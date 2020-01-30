using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using CBS.Pages;
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
            if(Request.Query.TryGetValue("teeTimeTime", out Microsoft.Extensions.Primitives.StringValues teeTimeTime))
            {
                if(long.TryParse(teeTimeTime.ToString(), out long teeTimeTicks))
                {
                    TeeTimeToUpdate = HttpContext.Session.Get<IEnumerable<TeeTime>>("reservedTeeTimes")?
                        .Where(t => t.Datetime.Ticks == teeTimeTicks).FirstOrDefault();
                }
                if ((TeeTimeToUpdate is null) || User.IsInRole("Golfer") && TeeTimeToUpdate.ReservedBy != userManager.GetUserId(User))
                    return NotFound();

                TempData.Put(nameof(TeeTimeToUpdate), TeeTimeToUpdate);
                return Page();
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult OnPost(string[] golfers, bool checkedIn)
        {
            if (!ModelState.IsValid) return Page();
            bool confirmation;
            string message;

            //Gather the names, UserId's and member number together. Need to see of the userId was not found, indicating an invalid entry
            var golfersToAdd = from suppliedMember in golfers
                               join user in userManager.Users on suppliedMember equals user.MemberNumber into foundMembers
                               from subMember in foundMembers.DefaultIfEmpty()
                               select (subMember?.MemberName, UserId: subMember?.Id, SuppliedNumber: suppliedMember);
            if(golfersToAdd.Any(g => string.IsNullOrEmpty(g.UserId)))
            {
                TempData.Put("errorMessage", "One or more supplied members do not exist");
                return Page();
            }
            new Helpers().UpdateTeeTimeHelper((from golfer in golfersToAdd select golfer.UserId).ToArray(), checkedIn, TempData.Peek<TeeTime>(nameof(TeeTimeToUpdate)), 
                userManager, Phone, NumberOfCarts, out confirmation, out message);

            if (!confirmation)
            {
                message = message.Contains("PRIMARY KEY") ? "Cannot add duplicate golfer" : message.Contains("FOREIGN KEY") ? "One or more member numbers do not exist" : message;
                TempData.Put("errorMessage", message);
                return Page();
            }

            HttpContext.Session.SetString("success", "Tee time updated successfully");

            return Redirect("/Manage/MyTeeTimes");
        }

    }
}
