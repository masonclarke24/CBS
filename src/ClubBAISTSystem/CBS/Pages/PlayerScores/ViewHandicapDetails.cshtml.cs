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
        public string BestOfLabel { get; set; }
        public IActionResult OnGet()
        {
            return NotFound();
        }

        public IActionResult OnPost(string email, DateTime lastUpdated)
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            FoundHandicapReport = requestDirector.GetHandicapReport(email, lastUpdated);
            if (FoundHandicapReport is null)
                return NotFound();

            BestOfLabel = "Best of " + (new int[] { 5, 6 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "1" :
                new int[] { 7, 8 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "2" :
                new int[] { 9, 10 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "3" :
                new int[] { 11, 12 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "4" :
                new int[] { 13, 14 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "5" :
                new int[] { 15, 16 }.Contains(FoundHandicapReport.PreviousRounds.Length) ? "6" :
                FoundHandicapReport.PreviousRounds.Length == 17 ? "7" :
                FoundHandicapReport.PreviousRounds.Length == 18 ? "8" :
                FoundHandicapReport.PreviousRounds.Length == 19 ? "9" : "10") + " rounds:";
            return Page();
        }
    }
}