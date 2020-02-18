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
        [Required, Range(0,double.MaxValue)]
        public double CourseRating { get; set; }
        [Required, Range(55,155)]
        public double CourseSlope { get; set; }
        [Required, PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
        public DateTime Date { get; set; }
        public int?[] HoleByHoleScores { get; set; } = new int?[18];
        public void OnGet()
        {

        }

        public IActionResult OnPost(int numberOfHoles)
        {
            if (!ModelState.IsValid) return Page();

            ScoreCard providedScores = new ScoreCard(Course, CourseRating, CourseSlope, Date, User.Identity.Name, 
                HoleByHoleScores.TakeWhile(s => !(s is null)).Cast<int>().ToList());

            bool confirmation = new Domain.CBS(Startup.ConnectionString).RecordScores(providedScores, out string message);

            if (confirmation)
            {
                HttpContext.Session.SetString("success", "Scores recored successfully");
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, message);
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