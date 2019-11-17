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
        public DailyTeeSheet DailyTeeSheet { get; set; }
        [BindProperty, Required]
        public DateTime Date { get; set; }
        public void OnGet()
        {

        }

        public void OnPostView()
        {
            if (ModelState.IsValid)
            {
                var signedInUser = userManager.FindByNameAsync(User.Identity.Name);
                signedInUser.Wait();
                TechnicalServices.CBS requestDirector = new TechnicalServices.CBS(signedInUser.Result.MemberNumber);
                DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);
            }
        }
    }
}