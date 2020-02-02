using Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TechnicalServices;
using TechnicalServices.Memberships;
using Xunit;


namespace CBSAutomatedTests
{
    public class TechnicalServicesTests:IDisposable
    {
        
        private readonly Random random;
        private readonly string connectionString = "Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;";
        private MembershipApplication membershipApplication;
        public TechnicalServicesTests()
        {
            random = new Random();
            string firstName = RandomName();
            string lastName = RandomName();
            membershipApplication = new MembershipApplication(connectionString)
            {
                ProspectiveMemberContactInfo = new ContactInformation()
                {

                    LastName = lastName,
                    FirstName = firstName,
                    Address = "12345 67 St",
                    PostalCode = "T1T 1T1",
                    City = "Edmonton",
                    PrimaryPhone = "4983069840",
                    AlternatePhone = "4983386910",
                    Email = $"{firstName}.{lastName}@test.com".ToLower()
                },
                DateOfBirth = DateTime.Parse("05-Jan-1998"),
                Occupation = "Business Analyst",
                EmploymentDetails = new ContactInformation()
                {
                    CompanyName = "CGI",
                    Address = "12345 Jasper Ave",
                    City = "Edmonton",
                    PostalCode = "T2T 2T2",
                    PrimaryPhone = "7809912176"
                },
                Shareholder1 = RandomName(),
                Shareholder2 = RandomName(),
                Shareholder1SigningDate = DateTime.Parse("30-Jan-2020"),
                Shareholder2SigningDate = DateTime.Parse("30-Jan-2020"),
                ProspectiveMemberCertification = true,
                ShareholderCertification = true,
                MembershipType = TechnicalServices.MembershipType.Shareholder
            };
        }

        [Fact]
        public void MembershipApplications_WhenRetrived_NotNull()
        {
            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            var membershipApplications = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-29), DateTime.Today.AddDays(1));
            Assert.NotNull(membershipApplications);
        }

        [Fact]
        public void MembershipApplication_Record_ReturnSuccess()
        {

            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            bool success = requestDirector.RecordMembershipApplication(membershipApplication);
            Assert.True(success);
        }

        [Fact]
        public void MembershipApplication_Recorded_ExistsWhenRetrieved()
        {
            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            requestDirector.RecordMembershipApplication(membershipApplication);
            var membershipApplications = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-29), DateTime.Today.AddDays(1));

            Assert.True(membershipApplications.Exists(ma => ma.ProspectiveMemberContactInfo.LastName == membershipApplication.ProspectiveMemberContactInfo.LastName
            && ma.ProspectiveMemberContactInfo.FirstName == membershipApplication.ProspectiveMemberContactInfo.FirstName));
            
        }
        [Fact]
        public void MembershipApplication_FilteredForNew_ContainsOnlyNew()
        {
            List<MembershipApplication> membershipApplications = new List<MembershipApplication>();

            for(int i = 0; i < 5; i++)
            {
                var changedApplication = new MembershipApplication(connectionString);
                changedApplication.ApplicationStatus = ApplicationStatus.Denied;
                membershipApplications.Add(changedApplication);
            }

            for (int i = 0; i < 5; i++)
            {
                var changedApplication = new MembershipApplication(connectionString);
                changedApplication.ApplicationStatus = ApplicationStatus.OnHold;
                membershipApplications.Add(changedApplication);
            }

            for (int i = 0; i < 5; i++)
            {
                var changedApplication = new MembershipApplication(connectionString);
                changedApplication.ApplicationStatus = ApplicationStatus.New;
                membershipApplications.Add(changedApplication);
            }

            Assert.True(membershipApplications.Count == 15);


            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            List<MembershipApplication> filteredMembershipApplications = 
            requestDirector.FilterMembershipApplications(membershipApplications, ApplicationStatus.New);

            Assert.True(filteredMembershipApplications.TrueForAll(q => q.ApplicationStatus == ApplicationStatus.New));
            Assert.True(filteredMembershipApplications.Count(q => q.ApplicationStatus == ApplicationStatus.New) == 5);
        }

        [Fact]
        public void MembershipApplication_WhenUpdated_Persist()
        {
            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            var applicationToUpdate = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-29), DateTime.Today.AddDays(1)).FirstOrDefault();

            Assert.NotNull(applicationToUpdate);
            ApplicationStatus initialStatus = applicationToUpdate.ApplicationStatus;

            bool confirmation = applicationToUpdate.UpdateMembershipApplication(ApplicationStatus.OnHold, out string message);

            Assert.True(confirmation);

            var retrievedApplication = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-29), DateTime.Today.AddDays(1)).Where(ma =>
            ma.ApplicationStatus == ApplicationStatus.OnHold && ma.ProspectiveMemberContactInfo.FirstName == applicationToUpdate.ProspectiveMemberContactInfo.FirstName
            && ma.ProspectiveMemberContactInfo.LastName == applicationToUpdate.ProspectiveMemberContactInfo.LastName).FirstOrDefault();

            Assert.NotNull(retrievedApplication);

            retrievedApplication.UpdateMembershipApplication(initialStatus, out string _);
        }

        [Fact]
        public void MembershipApplication_NoSuchMemberOnUpdate_False()
        {
            Assert.False(new MembershipApplication(connectionString).UpdateMembershipApplication(ApplicationStatus.Denied, out string _));
        }

        [Theory]
        [InlineData(MembershipType.Associate)]
        [InlineData(MembershipType.Shareholder)]
        public void CreateAccount_AccountExistsAndMatchesApplicationInfo(MembershipType membershipType)
        {
            membershipApplication = CreateAndRecordMembershipApplication(membershipType);

            Assert.NotNull(membershipApplication);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM AspNetUsers WHERE MemberName = '{membershipApplication.ProspectiveMemberContactInfo.FirstName} {membershipApplication.ProspectiveMemberContactInfo.LastName}'" +
                    $" AND Email = '{membershipApplication.ProspectiveMemberContactInfo.Email}' AND PhoneNumber = '{membershipApplication.ProspectiveMemberContactInfo.PrimaryPhone}' AND" +
                    $" MembershipType = '{membershipType.ToString()}'", connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    Assert.Equal("0", command.ExecuteScalar().ToString());
                }
            }

            bool confirmation = membershipApplication.CreateMemberAccount(out string message);

            Assert.True(confirmation);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM AspNetUsers WHERE MemberName = '{membershipApplication.ProspectiveMemberContactInfo.FirstName} {membershipApplication.ProspectiveMemberContactInfo.LastName}'" +
                    $" AND Email = '{membershipApplication.ProspectiveMemberContactInfo.Email}' AND PhoneNumber = '{membershipApplication.ProspectiveMemberContactInfo.PrimaryPhone}'", connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    Assert.Equal("1", command.ExecuteScalar().ToString());
                }
            }
        }

        private void RemoveMemberAccount()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand($"DELETE AspNetUsers WHERE MemberName = '{membershipApplication.ProspectiveMemberContactInfo.FirstName} {membershipApplication.ProspectiveMemberContactInfo.LastName}'" +
                    $"AND Email = '{membershipApplication.ProspectiveMemberContactInfo.Email}' AND PhoneNumber = '{membershipApplication.ProspectiveMemberContactInfo.PrimaryPhone}'", connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private MembershipApplication CreateAndRecordMembershipApplication(MembershipType membershipType)
        {
            Domain.CBS requestDirector = new Domain.CBS(connectionString);
            membershipApplication.MembershipType = membershipType;
            bool success = requestDirector.RecordMembershipApplication(membershipApplication);
            var foundApplication = requestDirector.GetMembershipApplications(DateTime.Today.AddDays(-29), DateTime.Today.AddDays(1)).FirstOrDefault();
            return foundApplication;
        }

        [Theory]
        [InlineData(MembershipType.Associate, 4500)]
        [InlineData(MembershipType.Shareholder, 3000)]
        public void CreateAccount_CorrectFeesAssessed(MembershipType type, double membershipFee)
        {
            membershipApplication = CreateAndRecordMembershipApplication(type);
            membershipApplication.CreateMemberAccount(out string message);

            string userId = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand($"SELECT Id FROM AspNetUsers WHERE MemberName = '{membershipApplication.ProspectiveMemberContactInfo.FirstName} {membershipApplication.ProspectiveMemberContactInfo.LastName}'" +
                    $" AND Email = '{membershipApplication.ProspectiveMemberContactInfo.Email}' AND PhoneNumber = '{membershipApplication.ProspectiveMemberContactInfo.PrimaryPhone}'", connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    userId = command.ExecuteScalar().ToString();
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand($"SELECT Amount FROM AccountTransactions WHERE Description = 'Membership fee' AND UserId = '{userId}'", connection))
                {
                    connection.Open();
                    Assert.Equal(membershipFee, double.Parse(command.ExecuteScalar().ToString()));
                }
            }
        }
        private string RandomName(int length = 10)
        {
            StringBuilder name = new StringBuilder();

            name.Append((char)random.Next('A', 'Z' + 1));

            for (int i = 0; i < length - 1; i++)
            {
                name.Append((char)random.Next('a', 'z' + 1));
            }

            return name.ToString();
        }
        private void RemoveMembershipApplication()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand($"DELETE MembershipApplication WHERE LastName = '{membershipApplication.ProspectiveMemberContactInfo.LastName}' AND FirstName = '{membershipApplication.ProspectiveMemberContactInfo.FirstName}'", connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            RemoveMemberAccount();
            RemoveMembershipApplication();
        }
    }
}
