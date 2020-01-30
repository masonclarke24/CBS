using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CBS
{
    [Authorize(Roles = "Shareholder")]
    public class MaxStandingTeeTimeRequestModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public MaxStandingTeeTimeRequestModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (requestDirector.FindStandingTeeTimeRequest(userManager.GetUserId(User)) is null)
                return NotFound();
            return Page();
        }
    }
}