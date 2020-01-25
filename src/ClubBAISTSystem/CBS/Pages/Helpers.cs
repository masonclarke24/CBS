using CBS.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TechnicalServices;

namespace CBS.Pages
{
    internal class Helpers
    {
        public Helpers()
        {

        }

        public void CancelTeeTimeHandler(string teeTime, string userId, ISession session, string sessionKey, UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
        {
            bool confirmation = false;
            var allTeeTimes = session.Get<IEnumerable<TeeTime>>(sessionKey);
            if (long.TryParse(teeTime, out long teeTimeTicks))
            {
                TeeTime teeTimeToCancel = allTeeTimes?.FirstOrDefault(t => t.Datetime.Ticks == teeTimeTicks);
                Domain.CBS requestDirector = new Domain.CBS(userId, Startup.ConnectionString);

                if (teeTimeToCancel != null)
                {
                    if ((user.IsInRole("Golfer") && teeTimeToCancel.Golfers.Exists(q => q.UserId == userManager.GetUserId(user)))
                                || teeTimeToCancel.Golfers.Exists(q => q.UserId == userId))
                        confirmation = requestDirector.CancelTeeTime(new DateTime(teeTimeTicks)); 
                }

                if (confirmation)
                    session.SetString("success", $"{(teeTimeToCancel.ReservedBy == userId ? "Tee Time cancelled" : "Golfer removed ")} successfully");
            }
            else
                session.SetString("danger", "Tee time could not be canceled");
        }

        public void UpdateTeeTimeHelper(string[] golfers, bool checkedIn, TeeTime teeTimeToUpdate, UserManager<ApplicationUser> userManager, string phone, int numberOfCarts, out bool confirmation, out string message)
        {
            Domain.CBS requestDirector = new Domain.CBS(Startup.ConnectionString);

            List<string> updatedGolfers = golfers.Length > 4 - teeTimeToUpdate.Golfers.Count ? golfers.ToList()
                .GetRange(0, 4 - teeTimeToUpdate.Golfers.Count) : golfers.ToList();

            updatedGolfers = (from user in userManager.Users where updatedGolfers.Contains(user.MemberNumber) select user.Id).ToList();

            confirmation = requestDirector.UpdateTeeTime(teeTimeToUpdate.Datetime, phone, numberOfCarts,
                updatedGolfers, checkedIn, out message);
        }

        public void CheckInHelper(string teeTime, string userId, ISession session)
        {
            if (long.TryParse(teeTime, out long teeTimeTicks))
            {
                Domain.CBS requestDirector = new Domain.CBS(userId, Startup.ConnectionString);
                if (requestDirector.UpdateTeeTime(new DateTime(teeTimeTicks), null, null, new List<string>(), true, out string message))
                    session.SetString("success", "Tee time checked in successfully");
            }
            else
                session.SetString("danger", "Failed to check in tee time");
        }
    }
}