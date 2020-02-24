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
    [Authorize(Policy = "ViewHandicapReport")]
    public class ViewHandicapReportModel : PageModel
    {
        [BindProperty, Display(Name = "Report Date"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}"), PageRemote(AdditionalFields = "__RequestVerificationToken", HttpMethod = "POST", PageHandler = "CheckDate")]
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
                FilteredHandicapReports = HttpContext.Session.Get<IEnumerable<dynamic>>("AllHandicapReports").Where(r => r["LastUpdated"].Value.Month == ReportDate.Month
                && r["LastUpdated"].Value.Year == ReportDate.Year && r["Email"].Value != User.Identity.Name);
            }
        }
        public void OnPost()
        {
            FilteredHandicapReports = from report in HttpContext.Session.Get<IEnumerable<dynamic>>("AllHandicapReports")
                                      where report["LastUpdated"].Value.Month == ReportDate.Month && report["LastUpdated"].Value.Year == ReportDate.Year
                                      && (report["Email"].Value.Contains(FilterCriteria ?? "") || report["MemberName"].Value.Contains(FilterCriteria ?? ""))
                                      select report;
        }

        public JsonResult OnPostCheckDate()
        {
            if ((ReportDate.Date - DateTime.Today).Days > 0)
                return new JsonResult("Date cannot be in the future");
            return new JsonResult(true);
        }

    }
}