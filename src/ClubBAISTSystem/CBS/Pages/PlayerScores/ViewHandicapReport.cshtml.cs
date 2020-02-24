using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices.PlayerScores;

namespace CBS
{
    [Authorize(Policy = "ViewMemberAccount")]
    public class ViewHandicapReportModel : PageModel
    {
        [BindProperty, Display(Name = "Report Date"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime ReportDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public HandicapReport FoundHandicapReport { get; set; }
        [BindProperty]
        public string FilterCriteria { get; set; }
        public IEnumerable<dynamic> FilteredHandicapReports { get; private set; }

        public void OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (User.IsInRole("Golfer"))
                FoundHandicapReport = requestDirector.GetHandicapReport(User.Identity.Name, ReportDate);

            var allHandicapReports = requestDirector.GetAllHandicapReports();
            if (allHandicapReports != null)
            {
                HttpContext.Session.Put("AllHandicapReports", from report in allHandicapReports select new { report.MemberName, report.Email, report.HandicapFactor, report.Average, report.BestOfTenAverage, report.LastUpdated });
                FilteredHandicapReports = HttpContext.Session.Get<IEnumerable<dynamic>>("AllHandicapReports").Where(r => r["LastUpdated"].Value.Month == ReportDate.Month && r["LastUpdated"].Value.Year == ReportDate.Year); 
            }
        }
        public void OnPost()
        {
            FilteredHandicapReports = HttpContext.Session.Get<IEnumerable<dynamic>>("AllHandicapReports");
        }

    }
}