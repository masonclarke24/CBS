using System;
using TechnicalServices;
namespace CBSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var membershipMgr = new TechnicalServices.Memberships.ClubMemberships("Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;");
            var membershipApplication = new TechnicalServices.Memberships.MembershipApplication();

            membershipApplication.ProspectiveMemberContactInfo = new ContactInformation()
            {
                LastName = "Williams",
                FirstName = "Smith",
                Address = "12345 67 St",
                City = "Edmonton",
                PostalCode = "T1T 1T1",
                PrimaryPhone = "5875636574",
                SecondaryPhone = "7804343625",
                Email = "jane.williams@test.com"
            };
            membershipApplication.DateOfBirth = DateTime.Parse("January 01, 1991");
            membershipApplication.MembershipType = MembershipType.Shareholder;
            membershipApplication.Occupation = "Business Analyst";
            membershipApplication.EmploymentDetails = new ContactInformation()
            {
                CompanyName = "CGI",
                Address = "12345 Jasper Ave",
                City = "Edmonton",
                PostalCode = "T2T 2T2",
                PrimaryPhone = "7803253452"
            };

            membershipApplication.ProspectiveMemberCertification = true;
            membershipApplication.Shareholder1 = "John Doe";
            membershipApplication.Shareholder1SigningDate = DateTime.Today;
            membershipApplication.Shareholder2 = "Johnathan Smith";
            membershipApplication.Shareholder2SigningDate = DateTime.Today;
            membershipApplication.ShareholderCertification = true;

            bool success = membershipMgr.RecordMembershipApplication(membershipApplication);
            Console.WriteLine(success);
            Console.ReadKey();
        }
    }
}
