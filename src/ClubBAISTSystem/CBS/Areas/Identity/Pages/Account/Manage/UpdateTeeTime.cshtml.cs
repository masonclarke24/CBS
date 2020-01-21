using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;

namespace CBS.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class UpdateTeeTimeModel : PageModel
    {
        [BindProperty, Phone, Required]
        public string Phone { get; set; }
        [BindProperty, Range(1, 5), Required, Display(Name = "Number of Carts")]
        public int NumberOfCarts { get; set; }

        public TeeTime TeeTimeToUpdate;
        private readonly UserManager<ApplicationUser> userManager;

        public UpdateTeeTimeModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult OnGet()
        {
            //TODO Fix getallteetimes
            if(Request.Query.TryGetValue("teeTimeTime", out Microsoft.Extensions.Primitives.StringValues teeTimeTime))
            {
                if(long.TryParse(teeTimeTime.ToString(), out long teeTimeTicks))
                {
                    TeeTimeToUpdate = TempData.Peek<IEnumerable<TeeTime>>("reservedTeeTimes")
                        .Where(t => t.Datetime.Ticks == teeTimeTicks).FirstOrDefault();
                }
                if (TeeTimeToUpdate is null)
                    return NotFound();

                TempData.Put(nameof(TeeTimeToUpdate), TeeTimeToUpdate);
                return Page();
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult OnPost(string[] golfers)
        {
            if (!ModelState.IsValid) return Page();
            Domain.CBS requestDirector = new Domain.CBS(userManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id, Startup.ConnectionString);

            TeeTimeToUpdate = TempData.Peek<TeeTime>(nameof(TeeTimeToUpdate));

            List<string> updatedGolfers = golfers.Length > 4 - TeeTimeToUpdate.Golfers.Count ? golfers.ToList()
                .GetRange(0, 4 - TeeTimeToUpdate.Golfers.Count) : golfers.ToList();

            if (!requestDirector.UpdateTeeTime(TeeTimeToUpdate.Datetime, Phone, NumberOfCarts, 
                updatedGolfers, out string message))
            {
                TempData.Put("errorMessage", message);
                return Page();
            }

            HttpContext.Session.SetString("success", "Tee time updated successfully");

            return RedirectToPage("MyTeeTimes");
        }
    }
}
