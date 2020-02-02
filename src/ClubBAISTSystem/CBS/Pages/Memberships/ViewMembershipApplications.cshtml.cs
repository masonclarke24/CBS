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
        public List<MembershipApplication> MembershipApplications { get; set;}
        public void OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            MembershipApplications = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-30), DateTime.Today);
            HttpContext.Session.Put(nameof(MembershipApplications), MembershipApplications);
        }

        public PartialViewResult OnPostFilter([FromForm]int? applicationStatus)
        {
            MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));

            if(applicationStatus == -1)
                return Partial("_MembershipApplicationsPartial", MembershipApplications);

            MembershipApplications = new Domain.CBS(Startup.ConnectionString)
                .FilterMembershipApplications(MembershipApplications, (TechnicalServices.ApplicationStatus)applicationStatus);
            return Partial("_MembershipApplicationsPartial", MembershipApplications);
        }
    }
}