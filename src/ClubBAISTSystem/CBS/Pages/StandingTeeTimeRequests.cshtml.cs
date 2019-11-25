using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using CBSClasses;
using System.IO;

namespace CBS.Pages
{
    [Authorize(Roles = "Shareholder")]
    public class StandingTeeTimeRequestsModel : PageModel
    {
        private UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public StandingTeeTimeRequestsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        [BindProperty, TempData, Display(Name = "Day of week")]
        public DayOfWeek DayOfWeek { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true), TempData]
        public DateTime StartDate { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true), TempData]
        public DateTime EndDate { get; set; }
        public List<StandingTeeTime> STTRequests { get; set; }
        [BindProperty, Required, ArrayLength(3, ErrorMessage = "A standing tee time request requires exactly 3 additional members"), Distinct(ErrorMessage = "Supplied members must be unique")]
        public string[] SuppliedMemberNumbers { get; set; }
        public bool Confirmation { get; private set; }
        public bool ValidTimeSelected { get; private set; } = false;
        public DateTime InitialDate { get; set; }

        public void OnGet()
        {
            if (Request.Query.TryGetValue("selectedTime", out StringValues selectedTime)
                && (TempData.Peek<IEnumerable<string>>("PermissableTimes")?.Any(s => s == selectedTime) ?? false)
                && !string.IsNullOrEmpty(HttpContext.Session.GetString(nameof(DayOfWeek))))
            {
                ValidTimeSelected = true;
                TempData["selectedTime"] = selectedTime.ToString();
            }
        }

        public void OnPostView(string startDate, long endDate)
        {
            if (DateTime.TryParse(startDate, out DateTime dt))
            {
                StartDate = dt;
                if ((DateTime.Today.AddDays(8) - StartDate).Days > 0)
                    ErrorMessages.Add($"Start Date must be beyond { DateTime.Today.AddDays(7).ToLongDateString()}");
                if (endDate > 0)
                {
                    EndDate = new DateTime(endDate);
                    if (StartDate.DayOfWeek != EndDate.DayOfWeek)
                        ErrorMessages.Add($"Day of week for Start Date ({StartDate.DayOfWeek.ToString()}) does not match day of week for End Date({EndDate.DayOfWeek.ToString()})");
                }
                else
                    ErrorMessages.Add("Please select an End Date");
                if (!(ErrorMessages?.Any() ?? false))
                {
                    CBSClasses.CBS requestDirector = new CBSClasses.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id,
                            Startup.ConnectionString);
                    STTRequests = requestDirector.ViewStandingTeeTimeRequests(StartDate, new DateTime(endDate));
                    TempData.Put("PermissableTimes", from time in STTRequests where time.Members is null select time.RequestedTime.ToString("HH:mm"));
                    return;
                }
            }
            TempData.Put(nameof(ErrorMessages), ErrorMessages);

        }

        public IActionResult OnPostChangeDate()
        {
            byte[] buffer = new byte[1000];
            Request.Body.ReadAsync(buffer).GetAwaiter().GetResult();

            string requestBody = new string(buffer.TakeWhile(b => b != '\0').Select(b => (char)b).ToArray());

            System.Text.StringBuilder selectBox = new System.Text.StringBuilder();

            selectBox.Append("<select name='EndDate' class='text-danger'><option value=''>Please enter a valid date</option>");
            if (DateTime.TryParse(requestBody, out DateTime startDate))
            {
                selectBox.Clear();

                if((DateTime.Today.AddDays(8) - startDate).Days > 0)
                {
                    selectBox.Append($"<select name='EndDate' class='text-danger'><option value=''>Start Date must be beyond {DateTime.Today.AddDays(7).ToLongDateString()}</option>");
                }
                else
                {
                    TempData[nameof(StartDate)] = startDate;
                    DateTime endDate = startDate.AddDays(7);

                    selectBox.Append("<select name='EndDate'>");

                    for (int i = 0; i < 52; i++)
                    {
                        selectBox.Append($"<option value='{endDate.Ticks}'>{endDate.ToLongDateString()}</option>");
                        endDate = endDate.AddDays(7);
                    }

                    selectBox.Append("</select>"); 
                }
            }
            return new JsonResult(selectBox.ToString());

        }

        public IActionResult OnPostSubmit()
        {
            ErrorMessages.Clear();
            bool isError = false;
            var errors = from error in ModelState
                         where (error.Key == nameof(SuppliedMemberNumbers) || error.Key == nameof(StartDate) || error.Key == nameof(EndDate))&&
                         error.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid && error.Value.Errors.Any()
                         select error.Value.Errors;
            if (errors.Any())
            {
                ErrorMessages.AddRange(from errorMessage in errors select errorMessage into messages from m in messages select m.ErrorMessage);
                isError = true;
            }

            var signedInMember = userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult();
            CBSClasses.CBS requestDirector = new CBSClasses.CBS(signedInMember.Id, Startup.ConnectionString);
            StandingTeeTime requestedStandingTeeTime = new StandingTeeTime() { StartDate = StartDate, EndDate = EndDate, RequestedTime = DateTime.Parse(TempData.Peek("selectedTime").ToString()) };

            for (int i = 0; i < SuppliedMemberNumbers.Length; i++)
                SuppliedMemberNumbers[i] = SuppliedMemberNumbers[i].Trim();
            var userIds = from user in dbContext.Users where SuppliedMemberNumbers.Contains(user.MemberNumber) select user.Id;
            if (userIds.Count() != SuppliedMemberNumbers.Count())
            {
                ErrorMessages.Add("One or more supplied member numbers do not exist");
                isError = true;
            }

            if (SuppliedMemberNumbers.Contains(signedInMember.MemberNumber.Trim()))
            {
                ErrorMessages.Add("Do not enter your own member number");
                isError = true;
            }

            SuppliedMemberNumbers = SuppliedMemberNumbers.Append(signedInMember.MemberNumber).ToArray();
            

            if (StartDate.DayOfWeek != EndDate.DayOfWeek)
            {
                ErrorMessages.Add($"Day of week for start date ({StartDate.DayOfWeek}) must match day of week for end date ({EndDate.DayOfWeek})");
                isError = true;
            }

            requestedStandingTeeTime.Members = userIds.ToList();

            if(isError)
            {
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"]);
            }

            if (!requestDirector.RequestStandingTeeTime(requestedStandingTeeTime, out string message))
            {
                ErrorMessages.Add(message);
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"]);
            }
            Confirmation = true;
            return Page();
        }
    }
}