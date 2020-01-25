using System;
using System.Collections.Generic;
using System.Text;
using TechnicalServices;

namespace Domain
{
    public class CBS
    {
        private readonly string connectionString;

        public CBS(string memberNumber, string connectionString)
        {
            MemberNumber = memberNumber;
            this.connectionString = connectionString;
        }

        public CBS(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string MemberNumber { get; private set; }

        public DailyTeeSheet ViewDailyTeeSheet(DateTime date)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.FindDailyTeeSheet(date);
        }

        public IEnumerable<TeeTime> FilterDailyTeeSheet(DateTime date, IEnumerable<TeeTime> teeTimes)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.FilterDailyTeeSheet(date, MemberNumber, teeTimes);
        }
        public bool ReserveTeeTime(TeeTime requestedTeeTime, out string message)
        {
            bool confirmation;
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            confirmation = teeSheetManager.ReserveTeeTime(requestedTeeTime, out message);
            return confirmation;
        }

        public List<StandingTeeTime> ViewStandingTeeTimeRequests(DateTime startDate, DateTime endDate)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.ViewStandingTeeTimeRequests(startDate, endDate);
        }

        public bool RequestStandingTeeTime(StandingTeeTime requestedStandingTeeTime, out string message)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.RequestStandingTeeTime(requestedStandingTeeTime, out message);
        }

        public List<TeeTime> FindReservedTeeTimes()
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.FindReservedTeeTimes();
        }

        public bool CancelTeeTime(DateTime teeTimeTime)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.CancelTeeTime(teeTimeTime);
        }

        public bool UpdateTeeTime(DateTime teeTimeTime, string newPhone, int? newNumberOfCarts, List<string> newGolfers, bool? checkedIn, out string message)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber, connectionString);
            return teeSheetManager.UpdateTeeTime(teeTimeTime, newPhone, newNumberOfCarts, newGolfers, checkedIn, out message);
        }
    }
}
