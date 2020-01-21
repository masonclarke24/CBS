using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace CBS.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class MyTeeTimesModel : PageModel
    {
        private UserManager<ApplicationUser> userManager;
        public List<TechnicalServices.TeeTime> reservedTeeTimes;

        public MyTeeTimesModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id, 
                Startup.ConnectionString);
            reservedTeeTimes = requestDirector.FindReservedTeeTimes();
            TempData.Put(nameof(reservedTeeTimes), reservedTeeTimes);
        }

        public IActionResult OnPostCancel(string teeTime)
        {
            string memberNumber = userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id;
            Domain.CBS requestDirector = new Domain.CBS(memberNumber, Startup.ConnectionString);

            bool confirmation = requestDirector.CancelTeeTime(new DateTime(long.Parse(teeTime)));

            if (confirmation)
                HttpContext.Session.SetString("success", "Tee time cancelled successfully");
            else
                HttpContext.Session.SetString("danger", "Tee time could not be canceled");
            return Redirect(Request.Headers["referer"]);

        }
    }
}
