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
        public readonly UserManager<ApplicationUser> userManager;
        public List<TechnicalServices.TeeTime> reservedTeeTimes;

        public MyTeeTimesModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {
            if (User.IsInRole("Golfer"))
            {
                string userId = userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id;
                GetReservedTeeTimes(userId);
            }
        }

        public IActionResult OnPostCancel(string teeTime)
        {
            string userId = userManager.GetUserId(User);
            Domain.CBS requestDirector = new Domain.CBS(userId, Startup.ConnectionString);

            bool confirmation = requestDirector.CancelTeeTime(new DateTime(long.Parse(teeTime)));

            if (confirmation)
                HttpContext.Session.SetString("success", "Tee time cancelled successfully");
            else
                HttpContext.Session.SetString("danger", "Tee time could not be canceled");
            return Redirect(Request.Headers["referer"]);

        }

        public void OnPostProvideMemberNumber(string memberNumber)
        {
            GetReservedTeeTimes(userManager.Users.Where(u => u.MemberNumber == memberNumber).FirstOrDefault().Id);
        }

        private void GetReservedTeeTimes(string userId)
        {
            Domain.CBS requestDirector = new Domain.CBS(userId,
                                    Startup.ConnectionString);
            reservedTeeTimes = requestDirector.FindReservedTeeTimes();
            TempData.Put(nameof(reservedTeeTimes), reservedTeeTimes);
        }
    }
}
