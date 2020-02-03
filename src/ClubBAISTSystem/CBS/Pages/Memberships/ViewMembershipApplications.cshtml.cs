using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices.Memberships;

namespace CBS
{
    [Authorize(Policy = "ManageMemberships")]
    public class ViewMembershipApplicationsModel : PageModel
    {
        public List<MembershipApplication> MembershipApplications { get; set; }
        [TempData]
        public DateTime StartDate { get; set; }
        [TempData]
        public DateTime EndDate { get; set; }
        public void OnGet()
        {
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;

            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            MembershipApplications = requestDirector.GetMembershipApplications(StartDate, EndDate);
            HttpContext.Session.Put(nameof(MembershipApplications), MembershipApplications);
            TempData.Keep();
        }

        public PartialViewResult OnPostFilter([FromForm]int? applicationStatus)
        {
            MembershipApplications = new Domain.CBS(Startup.ConnectionString).GetMembershipApplications(StartDate, EndDate);
            TempData.Keep();
            if (applicationStatus is null)
            {
                return Partial("_MembershipApplicationsPartial", MembershipApplications);
            }

            try
            {
                (from int enumValue in Enum.GetValues(typeof(TechnicalServices.ApplicationStatus)) where enumValue == applicationStatus select enumValue).First();
            }
            catch (InvalidOperationException)
            {
                return Partial("_MembershipApplicationsPartial", MembershipApplications);
            }

                MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));
            MembershipApplications = new Domain.CBS(Startup.ConnectionString)
            .FilterMembershipApplications(MembershipApplications, (TechnicalServices.ApplicationStatus)applicationStatus);

            return Partial("_MembershipApplicationsPartial", MembershipApplications);
        }

        public PartialViewResult OnPostGetMembershipApplications([FromForm]string startDate, [FromForm]string endDate)
        {

            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (!DateTime.TryParse(startDate, out DateTime startDateDate) || !DateTime.TryParse(endDate, out DateTime endDateDate))
            {
                MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));
            }
            else
            {
                StartDate = startDateDate;
                EndDate = endDateDate;
                MembershipApplications = requestDirector.GetMembershipApplications(StartDate, EndDate);
            }

            HttpContext.Session.Put(nameof(MembershipApplications), MembershipApplications);
            TempData.Keep();
            return Partial("_MembershipApplicationsPartial", MembershipApplications);
        }

        public PartialViewResult OnPostDetails()
        {
            return Partial("_UpdateMembershipApplicationPartial");
        }
    }
}