using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CBS.Areas.Identity.Pages.Account.Manage
{
    public class MyTeeTimesModel : PageModel
    {
        private UserManager<ApplicationUser> userManager;
        public List<TechnicalServices.TeeTime> reservedTeeTimes;

        public MyTeeTimesModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;

        }

        public void OnGet()
        {
            Domain.CBS requestDirector = new Domain.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id, 
                Startup.ConnectionString);
            reservedTeeTimes = requestDirector.FindReservedTeeTimes();
        }
    }
}
