using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TechnicalServices.Memberships
{
    public class ClubMemberships
    {
        private readonly string connectionString;

        public ClubMemberships(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool RecordMembershipApplication(MembershipApplication newMembershipApplication)
        {
            bool confirmation;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("RecordMembershipApplication", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@lastName", newMembershipApplication.ProspectiveMemberContactInfo.LastName);
                    command.Parameters.AddWithValue("@firstName", newMembershipApplication.ProspectiveMemberContactInfo.FirstName);
                    command.Parameters.AddWithValue("@applicantAddress", newMembershipApplication.ProspectiveMemberContactInfo.Address);
                    command.Parameters.AddWithValue("@applicantCity", newMembershipApplication.ProspectiveMemberContactInfo.City);
                    command.Parameters.AddWithValue("@applicantPostalCode", newMembershipApplication.ProspectiveMemberContactInfo.PostalCode);
                    command.Parameters.AddWithValue("@applicantPhone", newMembershipApplication.ProspectiveMemberContactInfo.PrimaryPhone);
                    command.Parameters.AddWithValue("@applicantAlternatePhone", newMembershipApplication.ProspectiveMemberContactInfo.AlternatePhone);
                    command.Parameters.AddWithValue("@email", newMembershipApplication.ProspectiveMemberContactInfo.Email);
                    command.Parameters.AddWithValue("@dateOfBirth", newMembershipApplication.DateOfBirth);
                    command.Parameters.AddWithValue("@membershipType", newMembershipApplication.MembershipType.ToString());
                    command.Parameters.AddWithValue("@occupation", newMembershipApplication.Occupation);
                    command.Parameters.AddWithValue("@companyName", newMembershipApplication.EmploymentDetails.CompanyName);
                    command.Parameters.AddWithValue("@employerAddress", newMembershipApplication.EmploymentDetails.Address);
                    command.Parameters.AddWithValue("@employerCity", newMembershipApplication.EmploymentDetails.City);
                    command.Parameters.AddWithValue("@employerPostalCode", newMembershipApplication.EmploymentDetails.PostalCode);
                    command.Parameters.AddWithValue("@employerPhone", newMembershipApplication.EmploymentDetails.PrimaryPhone);
                    command.Parameters.AddWithValue("@prospectiveMemberCertification", newMembershipApplication.ProspectiveMemberCertification);
                    command.Parameters.AddWithValue("@sponsoringShareholder1", newMembershipApplication.Shareholder1);
                    command.Parameters.AddWithValue("@shareholder1SigningDate", newMembershipApplication.Shareholder1SigningDate);
                    command.Parameters.AddWithValue("@sponsoringShareholder2", newMembershipApplication.Shareholder2);
                    command.Parameters.AddWithValue("@shareholder2SigningDate", newMembershipApplication.Shareholder2SigningDate);
                    command.Parameters.AddWithValue("@shareholderCertification", newMembershipApplication.ShareholderCertification);
                    command.Parameters.Add(new SqlParameter("@returnCode", -1) { Direction = System.Data.ParameterDirection.ReturnValue });
                    connection.Open();

                    try
                    {
                        command.ExecuteNonQuery();
                        confirmation = command.Parameters[command.Parameters.Count - 1].Value.ToString() == "0";
                    }
                    catch (Exception ex)
                    {
                        confirmation = false;
                    }
                }

                return confirmation;
            }
        }

        public List<MembershipApplication> GetMembershipApplications(DateTime startDate, DateTime endDate)
        {
            List<MembershipApplication> foundMembershipApplications = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetMembershipApplications", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            foundMembershipApplications = new List<MembershipApplication>();
                            while (reader.Read())
                            {
                                MembershipApplication foundApplication = new MembershipApplication(connectionString,(DateTime)reader["ApplicationDate"])
                                {
                                    ProspectiveMemberContactInfo = new ContactInformation()
                                    {
                                        LastName = reader["LastName"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        Address = reader["ApplicantAddress"].ToString(),
                                        City = reader["ApplicantCity"].ToString(),
                                        PostalCode = reader["ApplicantPostalCode"].ToString(),
                                        PrimaryPhone = reader["ApplicantPhone"].ToString(),
                                        AlternatePhone = reader["ApplicantAlternatePhone"].ToString(),
                                        Email = reader["Email"].ToString()
                                    },
                                    DateOfBirth = (DateTime)reader["DateOfBirth"],
                                    MembershipType = (MembershipType)Enum.Parse(typeof(MembershipType), reader["MembershipType"].ToString()),
                                    Occupation = reader["Occupation"].ToString(),

                                    EmploymentDetails = new ContactInformation()
                                    {
                                        CompanyName = reader["CompanyName"].ToString(),
                                        Address = reader["EmployerAddress"].ToString(),
                                        City = reader["EmployerCity"].ToString(),
                                        PostalCode = reader["EmployerPostalCode"].ToString(),
                                        AlternatePhone = reader["EmployerPhone"].ToString()
                                    },

                                    ProspectiveMemberCertification = (bool)reader["ProspectiveMemberCertification"],
                                    Shareholder1 = reader["SponsoringShareholder1"].ToString(),
                                    Shareholder1SigningDate = (DateTime)reader["Shareholder1SigningDate"],
                                    Shareholder2 = reader["SponsoringShareholder2"].ToString(),
                                    Shareholder2SigningDate = (DateTime)reader["Shareholder2SigningDate"],
                                    ShareholderCertification = (bool)reader["ShareholderCertification"],
                                    ApplicationStatus = reader["ApplicationStatus"]
                                                        is DBNull ? ApplicationStatus.New : (ApplicationStatus)Enum.Parse(typeof(ApplicationStatus), reader["ApplicationStatus"].ToString())
                                };
                                foundMembershipApplications.Add(foundApplication);
                            }
                        }
                    }
                }
            }
            return foundMembershipApplications;
        }

        internal List<MembershipApplication> FilterMembershipApplications(List<MembershipApplication> membershipApplications, ApplicationStatus applicationStatus)
        {
            return membershipApplications.Where(ma => ma.ApplicationStatus == applicationStatus).ToList();
        }
    }
}
