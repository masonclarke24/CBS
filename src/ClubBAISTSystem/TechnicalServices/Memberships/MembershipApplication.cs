using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TechnicalServices.Memberships
{
    public class MembershipApplication
    {
        public ContactInformation ProspectiveMemberContactInfo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public MembershipType MembershipType { get; set; }
        public string Occupation { get; set; }
        public ContactInformation EmploymentDetails { get; set; }
        public bool ProspectiveMemberCertification { get; set; }
        public DateTime? ApplicationDate => applicationDate;
        public string Shareholder1 { get; set; }
        public string Shareholder2 { get; set; }
        public DateTime Shareholder1SigningDate { get; set; }
        public DateTime Shareholder2SigningDate { get; set; }
        public bool ShareholderCertification { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }

        private const string defaultPassword = "Baist123$";
        private readonly DateTime? applicationDate;
        [JsonProperty]
        private string connectionString;

        [JsonConstructor]
        public MembershipApplication(string connectionString, DateTime? applicationDate)
            :this(connectionString)
        {
            this.applicationDate = applicationDate;
        }

        public MembershipApplication(string connectionString)
        {
            this.connectionString = connectionString;
        }


        public bool UpdateMembershipApplication(ApplicationStatus newStatus, out string message)
        {
            bool confirmation = false;
            if(ApplicationDate is null)
            {
                message = "Invalid membership application";
                return confirmation;
            }
            message = "success";
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("UpdateMembershipApplication", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@lastName", ProspectiveMemberContactInfo.LastName);
                    command.Parameters.AddWithValue("@firstName", ProspectiveMemberContactInfo.FirstName);
                    command.Parameters.AddWithValue("@applicationDate", ApplicationDate);
                    command.Parameters.AddWithValue("@applicationStatus", newStatus.ToString());
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
                        message = ex.Message;
                    }
                }
            }

            return confirmation;
        }

        public bool CreateMemberAccount(out string message)
        {
            bool confirmation;
            message = "success";
            Guid id = default;
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("CreateMemberAccount", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@membershipType", MembershipType.ToString());
                    command.Parameters.AddWithValue("@lastName", ProspectiveMemberContactInfo.LastName);
                    command.Parameters.AddWithValue("@firstName", ProspectiveMemberContactInfo.FirstName);
                    command.Parameters.AddWithValue("@email", ProspectiveMemberContactInfo.Email);
                    command.Parameters.AddWithValue("@phoneNumber", ProspectiveMemberContactInfo.PrimaryPhone);
                    command.Parameters.AddWithValue("@passwordHash", new PasswordHasher().HashPassword(defaultPassword));

                    Random random = new Random();
                    StringBuilder securityStamp = new StringBuilder();
                    for (int i = 0; i < 32; i++)
                    {
                        char next = (char)random.Next('0', 'Z' + 1);

                        while (next >= 58 && next <= 64)
                        {
                            next = (char)random.Next('0', 'Z' + 1);
                        }

                        securityStamp.Append(next);
                    }

                    command.Parameters.AddWithValue("@securityStamp", securityStamp.ToString());
                    command.Parameters.AddWithValue("@concurrencyStamp", Guid.NewGuid());
                    id = Guid.NewGuid();
                    command.Parameters.AddWithValue("id", id);
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
                        message = ex.Message;
                    }
                }
            }

            confirmation = AssessMembsershipFees(id,out message);
            return confirmation;
        }

        private bool AssessMembsershipFees(Guid id, out string message)
        {
            bool confirmation = false;
            message = "Success";
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("AssessMembershipFees", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    MembershipFees fees = Activator.CreateInstance(typeof(MembershipFees).Assembly.ToString(), $"TechnicalServices.Memberships.{MembershipType.ToString()}").Unwrap() as MembershipFees;
                    command.Parameters.AddWithValue("@feeDetails", fees.FeeDetails);
                    command.Parameters.AddWithValue("@userId", id);
                    command.Parameters.Add(new SqlParameter("@returnCode", -1) { Direction = System.Data.ParameterDirection.ReturnValue });
                    connection.Open();

                    try
                    {
                        command.ExecuteNonQuery();
                        confirmation = command.Parameters[command.Parameters.Count - 1].Value.ToString() == "0";
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                        confirmation = false;
                    }
                }
            }

            return confirmation;
        }
    }
}
