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
    }
}