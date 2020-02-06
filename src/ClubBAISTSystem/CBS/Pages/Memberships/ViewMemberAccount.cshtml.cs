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
                FoundMemberAccount = new Domain.CBS(Startup.ConnectionString).GetAccountDetail(userManager.GetUserAsync(User).GetAwaiter().GetResult().Email, FromDate, ToDate);
            }
        }
    }
}