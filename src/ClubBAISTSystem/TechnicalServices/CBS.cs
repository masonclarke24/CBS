using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices
{
    public class CBS
    {
        private readonly string connectionString;

        public CBS(string memberNumber, string connectionString)
        {
            MemberNumber = memberNumber;
            this.connectionString = connectionString;
        }

        public string MemberNumber { get; private set; }

        public DailyTeeSheet ViewDailyTeeSheet(DateTime date)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.FindDailyTeeSheet(date);
        }

        public bool ReserveTeeTime(TeeTime requestedTeeTime, out string message)
        {
            bool confirmation;
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            confirmation = teeSheetManager.ReserveTeeTime(requestedTeeTime, out message);
            return confirmation;
        }

        public List<StandingTeeTime> ViewStandingTeeTimeRequests(DayOfWeek dayOfWeek)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.ViewStandingTeeTimeRequests(dayOfWeek);
        }

        public bool RequestStandingTeeTime(StandingTeeTime requestedStandingTeeTime, out string message)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.RequestStandingTeeTime(requestedStandingTeeTime, out message);
        }
    }
}
