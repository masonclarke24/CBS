using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CBS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechnicalServices;
using Microsoft.AspNetCore.Http;

namespace CBS.Pages
{
    [Authorize(Roles = "Golfer, ProShop, Clerk")]
    public class ReserveTeeTimeModel : PageModel
    {
        public UserManager<ApplicationUser> UserManager { get; private set; }
        private readonly ApplicationDbContext dbContext;


        public ReserveTeeTimeModel(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.dbContext = dbContext;

        }

        public List<string> ErrorMessages { get; set; } = new List<string>();
        [TempData]
        public bool Confirmation { get; set; }
        public DailyTeeSheet DailyTeeSheet { get; set; }
        [Required, BindProperty, DisplayFormat(DataFormatString = "{0:dddd MMMM dd, yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [BindProperty, Phone, Required]
        public string Phone { get; set; }
        [BindProperty, Range(1, 99), Required, Display(Name = "Number of Carts")]
        public int NumberOfCarts { get; set; }
        public List<int> MemberErrorIds { get; set; } = new List<int>();

        public IActionResult OnGet()
        {
            Confirmation = false;
            if (Request.Query.TryGetValue("teeTime", out Microsoft.Extensions.Primitives.StringValues teeTime))
            {
                if (long.TryParse(System.Web.HttpUtility.UrlDecode(teeTime.ToString()), out long result))
                {
                    DateTime selectedTime = new DateTime(result);
                    if (!TempData.Peek<IEnumerable<DateTime>>("PermissableTimes").Contains(selectedTime))
                        return Redirect("/ReserveTeeTime");
                    Date = selectedTime;
                    TempData["Date"] = selectedTime;
                }
                else
                    return Redirect("/ReserveTeeTime");
            }
            return Page();
        }

        public IActionResult OnPostSelect(string teeTime)
        {
            Confirmation = false;
            TempData["Date"] = Date;
            return Redirect("/ReserveTeeTime?teeTime=" + teeTime);
        }

        public IActionResult OnPostView()
        {
            foreach (var item in ModelState)
            {
                if (item.Key == nameof(Date) && item.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                {
                    ErrorMessages.Add(item.Value.Errors.FirstOrDefault().ErrorMessage);
                    TempData.Put(nameof(ErrorMessages), ErrorMessages);
                    return Redirect(Request.Headers["Referer"].ToString());
                }

            }
            Confirmation = false;

            Domain.CBS requestDirector = null;
            if (!User.IsInRole("Golfer"))
                requestDirector = new Domain.CBS(Startup.ConnectionString);
            else
                requestDirector = new Domain.CBS(GetUserId(), Startup.ConnectionString);
            DailyTeeSheet = requestDirector.ViewDailyTeeSheet(Date);

            if (User.IsInRole("Golfer"))
            {
                DailyTeeSheet.TeeTimes = requestDirector.FilterDailyTeeSheet(Date, DailyTeeSheet.TeeTimes).ToList();
            }
                HttpContext.Session.Put("AllTeeTimes", DailyTeeSheet.TeeTimes);
            

            IEnumerable<TeeTime> reservedTeeTimes = null;
            if (User.IsInRole("Golfer"))
                reservedTeeTimes = requestDirector.FindReservedTeeTimes().Where(t => (t.Datetime.Date - DateTime.Today).TotalDays > 0);
            else
                reservedTeeTimes = from teeTime in DailyTeeSheet.TeeTimes
                                   where !(teeTime.Golfers is null)
          && (teeTime.Datetime - DateTime.Now).TotalDays > 0
                                   select teeTime;

            HttpContext.Session.Put(nameof(reservedTeeTimes), reservedTeeTimes);

            TempData.Put("PermissableTimes", (from teeTime in DailyTeeSheet.TeeTimes
                where (teeTime.Golfers is null || teeTime.Golfers.Count != 4) && teeTime.Reservable && IsValidDate(teeTime.Datetime)
                    select teeTime.Datetime).Except(from time in reservedTeeTimes select time.Datetime));
            return Page();
        }

        public IActionResult OnPostReserve(string[] golfers)
        {
            if (!ModelState.IsValid)
            {
                foreach (var e in from err in ModelState.Values where err.Errors.Count > 0 select err)
                {
                    ErrorMessages.Add(e.Errors.FirstOrDefault()?.ErrorMessage);
                }
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            Confirmation = false;

            Domain.CBS requestDirector = null;

            string userId = "";
            if (User.IsInRole("Golfer"))
            {
                userId = GetUserId();
            }
            else
            {
                userId = GetUserId(golfers.FirstOrDefault());

                if(userId is null)
                {
                    ErrorMessages.Add($"Supplied member Number {golfers.FirstOrDefault()} does not exist");
                    Confirmation = false;
                    TempData.Put(nameof(ErrorMessages), ErrorMessages);
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                requestDirector = new Domain.CBS(userId, Startup.ConnectionString);
                var filteredTeeTimes = requestDirector.FilterDailyTeeSheet((DateTime)TempData.Peek(nameof(Date)),
                    HttpContext.Session.Get<IEnumerable<TeeTime>>("AllTeeTimes"));

                if (!filteredTeeTimes.FirstOrDefault(t => t.Datetime == (DateTime)TempData.Peek(nameof(Date)))?.Reservable ?? false)
                {
                    ErrorMessages.Add($"Cannot reserve tee time for member {golfers.FirstOrDefault()} due to membership level conflict.");
                    Confirmation = false;
                    TempData.Put(nameof(ErrorMessages), ErrorMessages);
                    return Redirect(Request.Headers["Referer"].ToString());
                }
            }

            requestDirector = new Domain.CBS(userId, Startup.ConnectionString);

            //Gather the names, UserId's and member number together. Need to see of the userId was not found, indicating an invalid entry
            var golfersToAdd = from suppliedMember in golfers
                               join user in UserManager.Users on suppliedMember equals user.MemberNumber into foundMembers
                               from subMember in foundMembers.DefaultIfEmpty() where subMember?.Id != userId
                               select (subMember?.MemberName, UserId: subMember?.Id, SuppliedNumber: suppliedMember);

            //Check for invalid members
            if(golfersToAdd.Any(g => g.UserId is null))
            {
                ErrorMessages.Add("One or more supplied members do not exist");
                Confirmation = false;
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }

            //Ensure that signed in user is added to this tee time
            
            var teeTimeOwner = UserManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            golfersToAdd = golfersToAdd.Append((teeTimeOwner.MemberName, teeTimeOwner.Id, teeTimeOwner.MemberNumber));
            

            if (!requestDirector.ReserveTeeTime(new TeeTime()
            {
                Golfers = (from golfer in golfersToAdd select (golfer.MemberName, golfer.UserId ?? golfer.SuppliedNumber)).ToList(),
                Datetime = (DateTime)TempData.Peek(nameof(Date)),
                NumberOfCarts = NumberOfCarts,
                Phone = Phone,
                ReservedBy = userId
            }, out string error))
            {
                error = error.Contains("PRIMARY KEY") ? "Cannot insert duplicate member" : error;
                ErrorMessages.Add(error);
                Confirmation = false;
                TempData.Put(nameof(ErrorMessages), ErrorMessages);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            else
            {
                Confirmation = true;
                HttpContext.Session.SetString("success", "Tee time reserved successfully");
            }
            return Page();
        }

        public IActionResult OnPostJoin(string teeTime)
        {
            string message = null;
            if (long.TryParse(teeTime, out long timeTicks))
            {
                DateTime time = new DateTime(timeTicks);

                if (TempData.Peek<IEnumerable<DateTime>>("PermissableTimes").Contains(time))
                {
                    if (new Domain.CBS(Startup.ConnectionString)
                        .UpdateTeeTime(time,
                        null, null, new List<string> { UserManager.GetUserId(User) }, null, out message))
                    {
                        HttpContext.Session.SetString("success", "Tee time joined successfully");
                        return Page();
                    }
                }
            }

            HttpContext.Session.SetString("danger", "Tee time could not be joined. " + message);
            return Page();
        }

        private List<(string Name, string UserId)> GenerateGolfers(dynamic validMembers)
        {
            List<(string Name, string UserId)> result = new List<(string Name, string UserId)>();

            foreach (var item in validMembers)
            {
                result.Add((item.MemberName, item.Id));
            }

            return result;
        }

        public IActionResult OnPostCancel(string teeTime, string userId)
        {
            new Helpers().CancelTeeTimeHandler(teeTime, userId, HttpContext.Session, "AllTeeTimes", UserManager, User);
            return Page();
        }

        public void OnPostCheckIn(string teeTime, string userId)
        {
            new Helpers().CheckInHelper(teeTime, userId, HttpContext.Session);
        }


        private string GetUserId()
        {
            return UserManager.FindByNameAsync(User.Identity.Name).GetAwaiter().GetResult().Id;
        }

        private string GetUserId(string memberNumber)
        {
            return UserManager.Users.Where(u => u.MemberNumber == memberNumber).FirstOrDefault()?.Id;
        }

        private bool IsValidDate(DateTime value)
        {
            if (value.Ticks == 0)
            {
                return false;
            }

            if ((DateTime.Now.AddDays(7) - value).TotalDays <= 0)
            {
                return false;
            }

            if ((DateTime.Today - value).TotalDays > 0)
            {
                return false;
            }

            if (!User.IsInRole("ProShop"))
            {
                if (value.Date == DateTime.Now.Date)
                {
                    return false;
                }
            }
            else if ((value.Date == DateTime.Now.Date) &&
                (DateTime.Now.TimeOfDay - value.TimeOfDay).TotalMinutes > 0)
            {
                return false;
            }

            return true;
        }

    }
}