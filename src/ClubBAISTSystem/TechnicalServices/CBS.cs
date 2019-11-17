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

        public bool ReserveTeeTime(TeeTime requestedTeeTime)
        {
            bool confirmation;
            DailyTeeSheets teeSheetManager = new DailyTeeSheets(MemberNumber);
            confirmation = teeSheetManager.ReserveTeeTime(requestedTeeTime);
            return confirmation;
        }
    }
}
