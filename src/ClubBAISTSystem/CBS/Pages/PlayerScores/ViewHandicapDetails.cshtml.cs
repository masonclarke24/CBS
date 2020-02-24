using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices.PlayerScores;

namespace CBS
{
    [Authorize(Policy = "ViewHandicapReport")]
    public class ViewHandicapDetailsModel : PageModel
    {
        public HandicapReport FoundHandicapReport { get; set; }
        public void OnGet()
        {

        }

        public IActionResult OnPost(string email, DateTime lastUpdated)
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            FoundHandicapReport = requestDirector.GetHandicapReport(email, lastUpdated);
            if (FoundHandicapReport is null)
                return NotFound();
            return Page();
        }
    }
}