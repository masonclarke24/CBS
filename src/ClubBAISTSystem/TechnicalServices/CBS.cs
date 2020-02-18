using System;
using System.Collections.Generic;
using System.Text;
using TechnicalServices;
using TechnicalServices.Memberships;
using TechnicalServices.PlayerScores;

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

        public bool RecordScores(ScoreCard providedScores, out string message)
        {
            PlayerScores scoreManager = new PlayerScores(connectionString, providedScores.Email);
            return scoreManager.RecordScores(providedScores, out message);
        }

        public List<StandingTeeTime> ViewStandingTeeTimeRequests(DateTime startDate, DateTime endDate)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.ViewStandingTeeTimeRequests(startDate, endDate);
        }

        public List<(string Name, string Email, double Balance)> ViewAllAccountsSummary()
        {
            ClubMemberships membershipManager = new ClubMemberships(connectionString);
            return membershipManager.ViewAllAccountsSummary();
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

        public StandingTeeTime FindStandingTeeTimeRequest(string userId)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.FindStandingTeeTimeRequest(userId);
        }

        public bool CancelStandingTeeTime(DateTime startDate, DateTime endDate, DateTime requestedTime)
        {
            StandingTeeTimeRequests standingTeeTimeManager = new StandingTeeTimeRequests(connectionString);
            return standingTeeTimeManager.CancelStandingTeeTime(startDate, endDate, requestedTime);
        }

        public bool RecordMembershipApplication(MembershipApplication membershipApplication)
        {
            ClubMemberships membershipManager = new ClubMemberships(connectionString);
            return membershipManager.RecordMembershipApplication(membershipApplication);
        }

        public List<MembershipApplication> GetMembershipApplications(DateTime startDate, DateTime endDate)
        {
            ClubMemberships membershipManager = new ClubMemberships(connectionString);
            return membershipManager.GetMembershipApplications(startDate, endDate);
        }

        public List<MembershipApplication> FilterMembershipApplications(List<MembershipApplication> membershipApplications, ApplicationStatus applicationStatus)
        {
            ClubMemberships membershipManager = new ClubMemberships(connectionString);
            return membershipManager.FilterMembershipApplications(membershipApplications, applicationStatus);
        }

        public MemberAccount GetAccountDetail(string email, DateTime dateTime1, DateTime dateTime2)
        {
            MemberAccount requestedAccount = new MemberAccount(email, connectionString);
            requestedAccount.GetAccountDetails(dateTime1, dateTime2);
            return requestedAccount;
        }
    }
}
