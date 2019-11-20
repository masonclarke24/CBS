using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        private UserManager<ApplicationUser> userManager;
        public ReserveTeeTimeModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public bool IsValidTeeTime { get; set; }
        public DailyTeeSheet DailyTeeSheet { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public int NumberOfCarts { get; set; }
        [BindProperty]
        public string[] Golfers { get; set; } = new string[0];

        public void OnGet()
        {
            if(Request.Query.TryGetValue("teeTime", out Microsoft.Extensions.Primitives.StringValues teeTime))
            {
                if(DateTime.TryParse(System.Web.HttpUtility.UrlDecode(teeTime.ToString()), out DateTime result))
                {
                    Date = result;
                    IsValidTeeTime = true;
                }
            }
        }

        public IActionResult OnPostSelect(string teeTime)
        {
            return Redirect("/ReserveTeeTime?teeTime=" + System.Web.HttpUtility.UrlEncode($"{Date.ToLongDateString()} {teeTime}"));
        }

        public IActionResult OnPostView()
        {
            if (ModelState.IsValid)
            {
                var signedInUser = userManager.FindByNameAsync(User.Identity.Name);
                signedInUser.Wait();
                TechnicalServices.CBS requestDirector = new TechnicalServices.CBS(signedInUser.Result.MemberNumber);
                DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);
            }

            return Page();
        }

        public void OnPostReserve(string[] golfers)
        {
            
        }
    }
}