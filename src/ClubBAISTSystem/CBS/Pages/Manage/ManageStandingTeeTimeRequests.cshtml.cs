using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;

namespace CBS
{
    [Authorize(Roles = "Shareholder")]
    public class ManageStandingTeeTimeRequestsModel : PageModel
    {
        public readonly UserManager<ApplicationUser> userManager;

        public ManageStandingTeeTimeRequestsModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public StandingTeeTime FoundStandingTeeTime { get; set; }
        public void OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            FoundStandingTeeTime = requestDirector.FindStandingTeeTimeRequest(userManager.GetUserId(User));
            HttpContext.Session.Put(nameof(FoundStandingTeeTime), FoundStandingTeeTime);
        }

        public IActionResult OnPostCancel()
        {
            FoundStandingTeeTime = HttpContext.Session.Get<StandingTeeTime>(nameof(FoundStandingTeeTime));

            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (requestDirector.CancelStandingTeeTime(FoundStandingTeeTime.StartDate, FoundStandingTeeTime.EndDate, FoundStandingTeeTime.RequestedTime))
                HttpContext.Session.SetString("success", "Standing tee time request cancelled succcessfully");
            else
                HttpContext.Session.SetString("danger", "Standing tee time request could not be cancelled");
            return LocalRedirect("/");
        }
    }
}