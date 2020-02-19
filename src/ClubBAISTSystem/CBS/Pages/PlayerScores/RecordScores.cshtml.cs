using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices.PlayerScores;

namespace CBS
{
    [BindProperties]
    public class RecordScoresModel : PageModel
    {
        [Required, StringLength(30)]
        public string Course { get; set; }
        [Required, Range(1,999), Display(Name = "Course Rating")]
        public double CourseRating { get; set; }
        [Required, Range(55,155), Display(Name = "Course Slope")]
        public double CourseSlope { get; set; }
        [Required, DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}"), PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
        public DateTime Date { get; set; } = DateTime.Now;
        [Required]
        public int NumberOfHoles { get; set; }
        [Range(-10 * 18, 10), Display(Name = "Hole n")]
        public int?[] HoleByHoleScores { get; set; }
        public void OnGet()
        {
            HoleByHoleScores = new int?[18];
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                HoleByHoleScores = new int?[18];
                return Page();
            }

            if (NumberOfHoles != 18 && NumberOfHoles != 9)
            {
                ModelState.AddModelError(string.Empty, "Holes per round must be 9 or 18");
                return Page();
            }

            ScoreCard providedScores = new ScoreCard(Course, CourseRating, CourseSlope, Date, User.Identity.Name, 
                HoleByHoleScores.TakeWhile(s => !(s is null)).Cast<int>().ToList());

            if(providedScores.HoleByHoleScore.Count != NumberOfHoles)
            {
                ModelState.AddModelError(string.Empty, $"Exactly {NumberOfHoles} scores must be recored for a {NumberOfHoles} round of golf. {providedScores.HoleByHoleScore.Count} scores were provided. " +
                    $"If a hole is not played, record par plus any handicap to which you are entitled for the remaining holes.");
                return Page();
            }

            bool confirmation = new Domain.CBS(Startup.ConnectionString).RecordScores(providedScores, out string message);

            if (confirmation)
            {
                HttpContext.Session.SetString("success", $"Scores recorded successfully{(string.IsNullOrWhiteSpace(message) ? ". Handicap report updated successfully" : "")}");
                if (!string.IsNullOrWhiteSpace(message))
                    HttpContext.Session.SetString("warning", message);
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, message.Contains("PRIMARY KEY") ? $"Cannot add duplicate socrecard entry. A scorecard already exists on {Date.ToString("dd-MMM-yyyy")}" : message);
            return Page();
        }

        public JsonResult OnPostCheckDate()
        {
           if ((Date.Date - DateTime.Today).Days > 0)
                return new JsonResult("Date cannot be in the future");
            return new JsonResult(true);
        }
    }
}