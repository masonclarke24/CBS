using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices
{
    public class CBS
    {
        public CBS(string memberNumber)
        {
            MemberNumber = memberNumber;
        }

        public string MemberNumber { get; private set; }

        public DailyTeeSheet ViewDailyTeeSheet(DateTime date)
        {
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber);
            return teeSheetManager.FindDailyTeeSheet(date);
        }

        public bool ReserveTeeTime(TeeTime requestedTeeTime, out string message)
        {
            bool confirmation;
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber);
            confirmation = teeSheetManager.ReserveTeeTime(requestedTeeTime, out message);
            return confirmation;
        }

        public bool VerifyMembersExist(string[] golfers, out List<string> invalidMembers)
        {
            return new DailyTeeSheets(MemberNumber).VerifyMembersExist(golfers, out invalidMembers);
        }
    }
}
