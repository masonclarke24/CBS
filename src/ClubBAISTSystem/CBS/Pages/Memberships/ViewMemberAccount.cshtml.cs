using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices.Memberships;

namespace CBS
{
    [Authorize(Policy = "ViewMemberAccount")]
    public class ViewMemberAccountModel : PageModel
    {
        public MemberAccount FoundMemberAccount { get; private set; }
        public UserManager<ApplicationUser> userManager;
        [TempData]
        public DateTime FromDate { get; set; }
        [TempData]
        public DateTime ToDate { get; set; }
        public IEnumerable<(string Name, string Email, double Balance)> AllAccountsSummary { get; private set; }

        public ViewMemberAccountModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {

            if (User.IsInRole("Golfer"))
            {
                GetMemberAccount(User.Identity.Name);
            }
            else
            {
                AllAccountsSummary = new Domain.CBS(Startup.ConnectionString).ViewAllAccountsSummary();
                HttpContext.Session.Put(nameof(AllAccountsSummary), AllAccountsSummary);
            }
        }

        public void OnGetFilterByDescription(string description)
        {
            FoundMemberAccount = HttpContext.Session.Get<MemberAccount>(nameof(FoundMemberAccount));
            if (FoundMemberAccount is null)
            {
                ModelState.AddModelError(string.Empty, "An error has occured");
                return;
            }
            FoundMemberAccount.FilterAccountDetails(description);
            HttpContext.Session.Put(nameof(FoundMemberAccount), FoundMemberAccount);
        }

        public void OnGetFilterByTimespan(DateTime fromDate, DateTime toDate)
        {
            FoundMemberAccount = HttpContext.Session.Get<MemberAccount>(nameof(FoundMemberAccount));
            if(FoundMemberAccount is null)
            {
                ModelState.AddModelError(string.Empty, "An error has occured");
                return;
            }
            if((fromDate - toDate).Days > 0)
            {
                ModelState.AddModelError(string.Empty, "'Start Date' cannot be beyond 'End Date'");
            }
            FoundMemberAccount.GetAccountDetails(fromDate, toDate);
            HttpContext.Session.Put(nameof(FoundMemberAccount), FoundMemberAccount);
        }

        public PartialViewResult OnPostFilterAccountSummary([FromForm] string filter)
        {
            AllAccountsSummary = HttpContext.Session.Get<IEnumerable<(string, string, double)>>(nameof(AllAccountsSummary));

            return Partial("./MemberAccountPartials/_SelectMemberAccountPartial",
                from summary in AllAccountsSummary where summary.Name.ToUpper().Contains(filter?.ToUpper() ?? "") 
                || summary.Email.ToUpper().Contains(filter.ToUpper() ?? "") select summary);
        }

        public void OnPostGetMemberAccount(string email)
        {
            GetMemberAccount(email);
        }

        private void GetMemberAccount(string email)
        {
            if (FromDate.Ticks == 0 || ToDate.Ticks == 0)
            {
                FromDate = DateTime.Today.AddDays(-30);
                ToDate = DateTime.Today;
            }
            FoundMemberAccount = new Domain.CBS(Startup.ConnectionString).GetAccountDetail(email, FromDate, ToDate);
            HttpContext.Session.Put(nameof(FoundMemberAccount), FoundMemberAccount);
        }
    }
}