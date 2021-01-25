using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TechnicalServices
{
    public class StandingTeeTimeRequests
    {
        private readonly string connectionString;

        public StandingTeeTimeRequests(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<StandingTeeTime> ViewStandingTeeTimeRequests(DateTime startDate, DateTime endDate)
        {
            List<StandingTeeTime> result = new List<StandingTeeTime>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand viewStandingTeeTimes = new SqlCommand("ViewStandingTeeTimeRequests", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    viewStandingTeeTimes.Parameters.AddWithValue("@startDate", startDate);
                    viewStandingTeeTimes.Parameters.AddWithValue("@endDate", endDate);

                    connection.Open();
                    using (SqlDataReader reader = viewStandingTeeTimes.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                StandingTeeTime standingTeeTime = new StandingTeeTime();
                                standingTeeTime.StartDate = reader["Start Date"] is DBNull ? default : DateTime.Parse(reader["Start Date"].ToString());
                                standingTeeTime.EndDate = reader["End Date"] is DBNull ? default : DateTime.Parse(reader["End Date"].ToString());
                                standingTeeTime.RequestedTime = DateTime.Parse(reader["Requested Time"].ToString());

                                if (result.LastOrDefault()?.RequestedTime == standingTeeTime.RequestedTime)
                                    standingTeeTime = result.Last();

                                if (!(reader["Member Name"] is DBNull))
                                {
                                    if (standingTeeTime.Members is null)
                                        standingTeeTime.Members = new List<string>();
                                    standingTeeTime.Members.Add(reader["Member Name"].ToString());
                                }

                                if (standingTeeTime.Members is null || standingTeeTime.Members.Count == 1)
                                    result.Add(standingTeeTime);
                            }
                        }
                    }

                    return result;
                }
            }
        }
        public bool RequestStandingTeeTime(StandingTeeTime requestedStandingTeeTime, out string message)
        {
            bool confirmation;
            message = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand requestStandingTeeTime = new SqlCommand("requestStandingTeeTime", connection) { CommandType = CommandType.StoredProcedure })
                {
                    requestStandingTeeTime.Parameters.AddWithValue("@startDate", requestedStandingTeeTime.StartDate);
                    requestStandingTeeTime.Parameters.AddWithValue("@endDate", requestedStandingTeeTime.EndDate);
                    requestStandingTeeTime.Parameters.AddWithValue("@requestedTime", requestedStandingTeeTime.RequestedTime);
                    requestStandingTeeTime.Parameters.AddWithValue("@submittedBy", requestedStandingTeeTime.SubmittedBy);

                    DataTable memberNumbers = new DataTable();
                    memberNumbers.Columns.Add("ID");
                    foreach (var item in requestedStandingTeeTime.Members)
                    {
                        memberNumbers.Rows.Add(item);
                    }
                    requestStandingTeeTime.Parameters.AddWithValue("@UserIds", memberNumbers);
                    SqlParameter parameterMessage = new SqlParameter("@message", SqlDbType.VarChar) { Size = 512, Direction = ParameterDirection.Output };
                    requestStandingTeeTime.Parameters.Add(parameterMessage);
                    SqlParameter returnCode = new SqlParameter("@returnCode", SqlDbType.Bit) { Direction = ParameterDirection.ReturnValue };
                    requestStandingTeeTime.Parameters.Add(returnCode);
                    connection.Open();

                    try
                    {
                        requestStandingTeeTime.ExecuteNonQuery();
                        confirmation = int.Parse(returnCode.Value.ToString()) == 0;
                    }
                    catch (Exception ex)
                    {
                        confirmation = false;
                        message = parameterMessage.Value?.ToString();
                    }
                }
            }

            return confirmation;
        }

        public StandingTeeTime FindStandingTeeTimeRequest(string userId)
        {
            StandingTeeTime foundStandingTeeTime = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand findStandingTeeTimeRequest = new SqlCommand("FindSTTR", connection) { CommandType = CommandType.StoredProcedure })
                {
                    findStandingTeeTimeRequest.Parameters.AddWithValue("@userId", userId);

                    connection.Open();

                    using (SqlDataReader reader = findStandingTeeTimeRequest.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            foundStandingTeeTime = new StandingTeeTime();
                            while (reader.Read())
                            {
                                foundStandingTeeTime.StartDate = DateTime.Parse(reader["Start Date"].ToString());
                                foundStandingTeeTime.EndDate = DateTime.Parse(reader["End Date"].ToString());
                                foundStandingTeeTime.RequestedTime = DateTime.Parse(reader["Requested Time"].ToString());
                                foundStandingTeeTime.SubmittedBy = reader["Submitted By"].ToString();

                                if (foundStandingTeeTime.Members is null)
                                    foundStandingTeeTime.Members = new List<string>();
                                foundStandingTeeTime.Members.Add(reader["Member Name"].ToString());
                            }
                        }
                    }
                }
            }

            return foundStandingTeeTime;
        }

        public bool CancelStandingTeeTime(DateTime startDate, DateTime endDate, DateTime requestedTime)
        {
            bool confirmation;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand findStandingTeeTimeRequest = new SqlCommand("CancelSTTR", connection) { CommandType = CommandType.StoredProcedure })
                {
                    findStandingTeeTimeRequest.Parameters.AddWithValue("@startDate", startDate.Date);
                    findStandingTeeTimeRequest.Parameters.AddWithValue("@endDate", endDate.Date);
                    findStandingTeeTimeRequest.Parameters.AddWithValue("@requestedTime", requestedTime.TimeOfDay.ToString());
                    findStandingTeeTimeRequest.Parameters.Add(new SqlParameter("@returnCode", -1) { Direction = ParameterDirection.ReturnValue });

                    connection.Open();

                    try
                    {
                        findStandingTeeTimeRequest.ExecuteNonQuery();
                        confirmation = (int)findStandingTeeTimeRequest.Parameters[findStandingTeeTimeRequest.Parameters.Count - 1].Value == 0;
                    }
                    catch (Exception ex)
                    {
                        confirmation = false;
                    }
                }
            }

            return confirmation;
        }
    }
}
