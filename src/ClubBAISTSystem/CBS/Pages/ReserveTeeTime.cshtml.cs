using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;

namespace CBS.Pages
{
    [Authorize]
    public class ReserveTeeTimeModel : PageModel
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }
        private readonly ApplicationDbContext dbContext;
        private string memberNumber = null;
        public ReserveTeeTimeModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.dbContext = dbContext;
            if (User is null) return;
            GetMemberNumber();
        }

        public List<string> ErrorMessages { get; set; } = new List<string>();
        [TempData]
        public bool Confirmation { get; set; }
        public DailyTeeSheet DailyTeeSheet { get; set; }
        [Required, BindProperty, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [BindProperty, Phone, Required]
        public string Phone { get; set; }
        [BindProperty, Range(1,99),Required, Display(Name = "Number of Carts")]
        public int NumberOfCarts { get; set; }
        public List<int> MemberErrorIds { get; set; } = new List<int>();

        public IActionResult OnGet()
        {
            Confirmation = false;
            if (Request.Query.TryGetValue("teeTime", out Microsoft.Extensions.Primitives.StringValues teeTime))
            {
                if (DateTime.TryParse(System.Web.HttpUtility.UrlDecode(teeTime.ToString()), out DateTime result))
                {
                    if(!TempData.Peek<IEnumerable<DateTime>>("PermissableTimes").Contains(result))
                        return Redirect("/ReserveTeeTime");
                    Date = result;
                    TempData["Date"] = result;
                }
                else
                    return Redirect("/ReserveTeeTime");
            }
            return Page();
        }

        public IActionResult OnPostSelect(string teeTime)
        {
            Confirmation = false;
            TempData["Date"] = Date;
            return Redirect("/ReserveTeeTime?teeTime=" + System.Web.HttpUtility.UrlEncode($"{Date.ToShortDateString()} {teeTime}"));
        }

        public IActionResult OnPostView()
        {
            foreach (var item in ModelState)
            {
                if (item.Key == nameof(Date) && item.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                {
                    ErrorMessages.Add(item.Value.Errors.FirstOrDefault().ErrorMessage);
                    TempData.Put(nameof(ErrorMessages), ErrorMessages);
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                
            }
            Confirmation = false;
                if (memberNumber is null)
                    GetMemberNumber();
                Domain.CBS requestDirector = new Domain.CBS(memberNumber, Startup.ConnectionString);
                DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);

            TempData.Put("PermissableTimes", from time in DailyTeeSheet.TeeTimes where time.Golfers is null && 
                time.Reservable && IsValidDate(time.Datetime, out string _) select time.Datetime);
            return Page();
        }

        public IActionResult OnPostReserve(string[] golfers)
        {
            var memberNumbers = golfers.Where(s => !(string.IsNullOrWhiteSpace(s)));
            if (!ModelState.IsValid)
            {
                foreach (var e in from err in ModelState.Values where err.Errors.Count > 0 select err)
                {
                    ErrorMessages.Add(e.Errors.FirstOrDefault()?.ErrorMessage);
                }
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            if (memberNumber is null) GetMemberNumber();
            Confirmation = false;
            Domain.CBS requestDirector = new Domain.CBS(memberNumber, Startup.ConnectionString);

            var validMembers = dbContext.Users.Where(u => memberNumbers.Contains(u.MemberNumber));
            if(validMembers.Count() != memberNumbers.Count())
            {
                ErrorMessages.Add("One or more provided member numbers do not exist");
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (!requestDirector.ReserveTeeTime(new TeeTime() { Golfers = validMembers.Select(M => M.Id).ToList().Append(memberNumber).ToList(), Datetime = (DateTime)TempData.Peek(nameof(Date)), NumberOfCarts = NumberOfCarts, Phone = Phone }, out string error))
            {
                ErrorMessages.Add(error);
                Confirmation = false;
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
            }
            else
                Confirmation = true;
            return Page();
        }

        private void GetMemberNumber()
        {
            var s = UserManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult();
            memberNumber = s.Id;
        }

        private bool IsValidDate(DateTime value, out string errorMessage)
        {
            errorMessage = "";
            if (value.Ticks == 0)
            {
                errorMessage = "Supplied date is invalid";
                return false;
            }

            if (value.Date == DateTime.Now.Date)
            {
                errorMessage = "Cannot reserve tee time for today";
                return false;
            }

            if ((DateTime.Now.AddDays(7) - value).TotalDays <= 0)
            {
                errorMessage = $"Selected day must not be beyond {DateTime.Today.AddDays(7).ToLongDateString()}";
                return false;
            }

            if ((DateTime.Now - value).TotalDays > 0)
            {
                errorMessage = "Selected day cannot be in the past";
                return false;
            }

            return true;
        }

    }
}