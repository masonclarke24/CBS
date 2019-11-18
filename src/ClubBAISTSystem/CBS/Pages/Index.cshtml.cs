using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CBS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<Data.ApplicationUser> userManager;

        public IndexModel(ILogger<IndexModel> logger, UserManager<Data.ApplicationUser> userManager)
        {
            _logger = logger;
            this.userManager = userManager;
        }

        public void OnGet()
        {
            var user = userManager.FindByNameAsync("test@test.com").GetAwaiter().GetResult();
            if(user is null)
            {
                userManager.CreateAsync(new Data.ApplicationUser() { MemberNumber = "1029384756", Email = "test@test.com" }, "Baist123$");
            }
        }
    }
}
