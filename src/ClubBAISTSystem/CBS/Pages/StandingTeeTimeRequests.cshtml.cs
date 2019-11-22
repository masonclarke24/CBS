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
using TechnicalServices;

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
        [BindProperty, TempData]
        public DayOfWeek DayOfWeek { get; set; }
        public List<StandingTeeTime> STTRequests { get; set; }
        [BindProperty, Required, ArrayLength(3, ErrorMessage = "A standing tee time request requires exactly 3 additional members")]
        public string[] SuppliedMemberNumbers { get; set; }
        public bool Confirmation { get; private set; }
        public bool ValidTimeSelected { get; private set; } = false;

        public void OnGet()
        {
            if (Request.Query.TryGetValue("selectedTime", out StringValues selectedTime)
                && TempData.Peek<IEnumerable<string>>("PermissableTimes").Any(s => s == selectedTime)
                && !string.IsNullOrEmpty(HttpContext.Session.GetString(nameof(DayOfWeek))))
            {
                ValidTimeSelected = true;
            }
        }

        public void OnPostView(DayOfWeek dayOfWeek)
        {
            HttpContext.Session.SetString(nameof(DayOfWeek), dayOfWeek.ToString());
            TechnicalServices.CBS requestDirector = new TechnicalServices.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id,
                Startup.ConnectionString);
            STTRequests = requestDirector.ViewStandingTeeTimeRequests(dayOfWeek);
            TempData.Put("PermissableTimes", from time in STTRequests where time.Members is null select time.RequestedTime.ToString("HH:mm"));
        }

        public IActionResult OnPostSelect(string selectedTime)
        {
            Confirmation = false;
            TempData["selectedTime"] = selectedTime;
            return Redirect($"/StandingTeeTimeRequests?selectedTime={System.Web.HttpUtility.UrlEncode(selectedTime)}");
        }

        public IActionResult OnPostSubmit()
        {
            var errors = from error in ModelState
                         where error.Key == nameof(SuppliedMemberNumbers) &&
                         error.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid && error.Value.Errors.Any()
                         select error.Value.Errors;
            if (errors.Any())
            {
                ErrorMessages.AddRange(from message in errors select message into messages from m in messages select m.ErrorMessage);
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"]);
            }

            return Page();
        }
    }
}