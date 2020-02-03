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

        public IActionResult OnPostGetMembershipApplications([FromForm]string startDate, [FromForm]string endDate)
        {

            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);
            if (!DateTime.TryParse(startDate, out DateTime startDateDate) || !DateTime.TryParse(endDate, out DateTime endDateDate))
            {
                MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));
            }
            else
            {
                if(startDateDate.Ticks == 0 || endDateDate.Ticks == 0)
                {
                    if(startDateDate.Ticks == 0)
                        ModelState.AddModelError(string.Empty, "Please select a valid start date");
                    if(EndDate.Ticks == 0)
                        ModelState.AddModelError(string.Empty, "Please select a valid start date");
                    return Page();
                }
                StartDate = startDateDate;
                EndDate = endDateDate;
                MembershipApplications = requestDirector.GetMembershipApplications(StartDate, EndDate);
            }

            HttpContext.Session.Put(nameof(MembershipApplications), MembershipApplications);
            TempData.Keep();
            return Partial("_MembershipApplicationsPartial", MembershipApplications);
        }

        public IActionResult OnPostDetails(string email, string applicationDate)
        {
            MembershipApplication foundMembershipApplication = null;
            if(long.TryParse(applicationDate, out long dateTicks))
            {
                MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));
                foundMembershipApplication = MembershipApplications.Find(ma => ma.ApplicationDate.GetValueOrDefault().Date.Ticks == dateTicks &&
                ma.ProspectiveMemberContactInfo.Email == email);
            }

            if(foundMembershipApplication is null)
            {
                ModelState.AddModelError(string.Empty, "Could not locate membership application");
                return Page();
            }

            return Partial("_UpdateMembershipApplicationPartial", foundMembershipApplication);
        }

        public IActionResult OnPostUpdate(int applicationStatus, string email, long applicationDate)
        {
            MembershipApplications = HttpContext.Session.Get<List<MembershipApplication>>(nameof(MembershipApplications));
            MembershipApplication foundMembershipApplication = MembershipApplications.Find(ma => ma.ApplicationDate.GetValueOrDefault().Date.Ticks == applicationDate &&
             ma.ProspectiveMemberContactInfo.Email == email);

            if(applicationStatus == (int)foundMembershipApplication.ApplicationStatus)
            {
                ModelState.AddModelError(string.Empty, "Please select a new application status");
                return Partial("_UpdateMembershipApplicationPartial", foundMembershipApplication);
            }

            if(foundMembershipApplication.ApplicationStatus == TechnicalServices.ApplicationStatus.Accepted)
            {
                ModelState.AddModelError(string.Empty, "Cannot revoke an accepted application");
                return Partial("_UpdateMembershipApplicationPartial", foundMembershipApplication);
            }

            if(foundMembershipApplication.UpdateMembershipApplication((TechnicalServices.ApplicationStatus)applicationStatus, out string message))
            {
                bool accountCreated = false;
                if(applicationStatus == (int)TechnicalServices.ApplicationStatus.Accepted)
                {
                    accountCreated = foundMembershipApplication.CreateMemberAccount(out message);
                }
                HttpContext.Session.Put("success", "Membership application updated successfully." + (accountCreated ? " An account has been created for this member." : ""));
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, "Could not update membership application");
            return Partial("_UpdateMembershipApplicationPartial", foundMembershipApplication);
        }
    }
}