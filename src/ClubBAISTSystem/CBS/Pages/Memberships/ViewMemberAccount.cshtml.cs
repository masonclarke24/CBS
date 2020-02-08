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

        public ViewMemberAccountModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {

            if (User.IsInRole("Golfer"))
            {
                if(FromDate.Ticks == 0 || ToDate.Ticks == 0)
                {
                    FromDate = DateTime.Today.AddDays(-30);
                    ToDate = DateTime.Today;
                }
                FoundMemberAccount = new Domain.CBS(Startup.ConnectionString).GetAccountDetail(User.Identity.Name, FromDate, ToDate);
                HttpContext.Session.Put(nameof(FoundMemberAccount), FoundMemberAccount);
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
    }
}