using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;

namespace CBS.Pages
{
    [Authorize]
    public class ReserveTeeTimeModel : PageModel
    {
        private UserManager<ApplicationUser> userManager;
        private string memberNumber = null;
        public ReserveTeeTimeModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            if (User is null) return;
            GetMemberNumber();
        }

        [TempData]
        public string ErrorMessage { get; set; }
        [TempData]
        public bool Confirmation { get; set; }
        public DailyTeeSheet DailyTeeSheet { get; set; }
        [BindProperty, Required, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public int NumberOfCarts { get; set; }
        public List<int> MemberErrorIds { get; set; } = new List<int>();

        public void OnGet()
        {
            Confirmation = false;
            if (Request.Query.TryGetValue("teeTime", out Microsoft.Extensions.Primitives.StringValues teeTime))
            {
                if (DateTime.TryParse(System.Web.HttpUtility.UrlDecode(teeTime.ToString()), out DateTime result))
                {
                    Date = result;
                    TempData["Date"] = result;
                }
            }
        }

        public IActionResult OnPostSelect(string teeTime)
        {
            Confirmation = false;
            TempData["Date"] = Date;
            return Redirect("/ReserveTeeTime?teeTime=" + System.Web.HttpUtility.UrlEncode($"{Date.ToShortDateString()} {teeTime}"));
        }

        public IActionResult OnPostView()
        {
            Confirmation = false;
                if (memberNumber is null)
                    GetMemberNumber();
                TechnicalServices.CBS requestDirector = new TechnicalServices.CBS(memberNumber);
                DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);
            return Page();
        }

        public IActionResult OnPostReserve(string[] golfers)
        {
            if (memberNumber is null) GetMemberNumber();
            Confirmation = false;
            TechnicalServices.CBS requestDirector = new TechnicalServices.CBS(memberNumber);

            if(!requestDirector.VerifyMembersExist(golfers, out List<string> invalidMembers))
            {
                ErrorMessage = $"The following members do not exist: {new string(invalidMembers.SelectMany(m => m.ToArray().Append(',').Append(' ')).ToArray())}";
                foreach (var invalid in invalidMembers)
                {
                    MemberErrorIds.Add(Array.IndexOf(golfers, invalid));
                }
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (!requestDirector.ReserveTeeTime(new TeeTime() { Golfers = golfers.ToList(), Datetime = (DateTime)TempData.Peek(nameof(Date)), NumberOfCarts = NumberOfCarts, Phone = Phone }, out string error))
            {
                ErrorMessage = error;
                Confirmation = false;
            }
            else
                Confirmation = true;
            return Page();
        }

        private void GetMemberNumber()
        {
            var signedInUser = userManager.FindByNameAsync(User.Identity.Name);
            signedInUser.Wait();
            memberNumber = signedInUser.Result.MemberNumber;
        }
    }
}