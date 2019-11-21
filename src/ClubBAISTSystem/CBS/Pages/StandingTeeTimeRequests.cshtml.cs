using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CBS.Pages
{
    [Authorize(Roles = "Shareholder")]
    public class StandingTeeTimeRequestsModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}