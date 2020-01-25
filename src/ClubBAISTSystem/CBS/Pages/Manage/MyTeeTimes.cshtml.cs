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
using CBS.Pages;

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
                GetReservedTeeTimes(userManager.GetUserId(User));
            }
        }

        public IActionResult OnPostCancel(string teeTime, string userId)
        {
            new Helpers().CancelTeeTimeHandler(teeTime, userId, HttpContext.Session, nameof(reservedTeeTimes), 
                userManager, User);
            GetReservedTeeTimes(userId);
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
            HttpContext.Session.Put(nameof(reservedTeeTimes), reservedTeeTimes);
        }
    }
}
