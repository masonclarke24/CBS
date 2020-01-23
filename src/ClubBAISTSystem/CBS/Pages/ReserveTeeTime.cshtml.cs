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
using Microsoft.AspNetCore.Http;

namespace CBS.Pages
{
    [Authorize]
    public class ReserveTeeTimeModel : PageModel
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }
        private readonly ApplicationDbContext dbContext;
 

        public ReserveTeeTimeModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.dbContext = dbContext;

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
            
            Domain.CBS requestDirector = null;
            if (!User.IsInRole("Golfer"))
                requestDirector = new Domain.CBS(Startup.ConnectionString);
            else
                requestDirector = new Domain.CBS(GetUserId(), Startup.ConnectionString);
            DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);

            if (User.IsInRole("Golfer"))
            {
                DailyTeeSheet.TeeTimes = requestDirector.FilterDailyTeeSheet(Date, DailyTeeSheet.TeeTimes).ToList();
            }
            else
            {
                TempData.Put("AllTeeTimes", DailyTeeSheet.TeeTimes);
            }
            TempData.Put("PermissableTimes", from time in DailyTeeSheet.TeeTimes where time.Golfers is null && 
                time.Reservable && IsValidDate(time.Datetime, out string _) select time.Datetime);

            if (User.IsInRole("Golfer"))
                TempData.Put("reservedTeeTimes", requestDirector.FindReservedTeeTimes().Where(t =>  (t.Datetime.Date - DateTime.Today).TotalDays > 0));
            else
                TempData.Put("reservedTeeTimes", from teeTime in DailyTeeSheet.TeeTimes where !(teeTime.Golfers is null) 
                                                 && (teeTime.Datetime.Date - DateTime.Today).TotalDays > 0 select teeTime);
            return Page();
        }

        public IActionResult OnPostReserve(string[] golfers)
        {
            if (!ModelState.IsValid)
            {
                foreach (var e in from err in ModelState.Values where err.Errors.Count > 0 select err)
                {
                    ErrorMessages.Add(e.Errors.FirstOrDefault()?.ErrorMessage);
                }
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            Confirmation = false;

            Domain.CBS requestDirector = null;

            string userId = "";
            if (User.IsInRole("Golfer"))
            {
                userId = GetUserId();
            }
            else
            {
                userId = GetUserId(golfers.FirstOrDefault());

                requestDirector = new Domain.CBS(userId, Startup.ConnectionString);
                var filteredTeeTimes = requestDirector.FilterDailyTeeSheet((DateTime)TempData.Peek(nameof(Date)), 
                    TempData.Peek<IEnumerable<TeeTime>>("ReservedTeeTimes"));

                if(filteredTeeTimes.Where(t => t.Datetime == (DateTime)TempData.Peek(nameof(Date))).FirstOrDefault() is null)
                {
                    ErrorMessages.Add($"Cannot reserve tee time from member {golfers.FirstOrDefault()} due to membership level conflict.");
                    Confirmation = false;
                    TempData.Put(nameof(ErrorMessages), ErrorMessages);
                    return Redirect(Request.Headers["Referer"].ToString());
                }
            }

            requestDirector = new Domain.CBS(userId, Startup.ConnectionString);

            var validMembers = dbContext.Users.Where(u => golfers.Contains(u.MemberNumber));

            validMembers = from member in validMembers where member.Id != userId select member;
            if (!requestDirector.ReserveTeeTime(new TeeTime() { Golfers = validMembers.Select(M => M.Id).ToList().Append(userId).ToList(), 
                Datetime = (DateTime)TempData.Peek(nameof(Date)), NumberOfCarts = NumberOfCarts, Phone = Phone }, out string error))
            {
                error = error.Contains("PRIMARY KEY") ? "Cannot insert duplicate member" : error;
                ErrorMessages.Add(error);
                Confirmation = false;
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                Confirmation = true;
                HttpContext.Session.SetString("success", "Tee time reserved successfully");
            }
            return Page();
        }

        public IActionResult OnPostCancel(string teeTime)
        {
            Domain.CBS requestDirector = new Domain.CBS(GetUserId(), Startup.ConnectionString);

            bool confirmation = requestDirector.CancelTeeTime(new DateTime(long.Parse(teeTime)));

            if (confirmation)
                HttpContext.Session.SetString("success", "Tee time cancelled successfully");
            else
                HttpContext.Session.SetString("danger", "Tee time could not be canceled");
            return Page();

        }

        private string GetUserId()
        {
            return UserManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id;
        }

        private string GetUserId(string memberNumber)
        {
            return UserManager.Users.Where(u => u.MemberNumber == memberNumber).FirstOrDefault().Id;
        }

        private bool IsValidDate(DateTime value, out string errorMessage)
        {
            errorMessage = "";
            if (value.Ticks == 0)
            {
                errorMessage = "Supplied date is invalid";
                return false;
            }

            if ((DateTime.Now.AddDays(7) - value).TotalDays <= 0)
            {
                errorMessage = $"Selected day must not be beyond {DateTime.Today.AddDays(7).ToLongDateString()}";
                return false;
            }

            if ((DateTime.Today - value).TotalDays > 0)
            {
                errorMessage = "Selected day cannot be in the past";
                return false;
            }

            if (!User.IsInRole("ProShop"))
            {
                if (value.Date == DateTime.Now.Date)
                {
                    errorMessage = "Cannot reserve tee time for today";
                    return false;
                }
            }
            else if ((value.Date == DateTime.Now.Date) && 
                (DateTime.Parse("January 22, 2020 15:30").TimeOfDay - value.TimeOfDay).TotalMinutes > 0)
            {
                return false;
            }

            return true;
        }

    }
}