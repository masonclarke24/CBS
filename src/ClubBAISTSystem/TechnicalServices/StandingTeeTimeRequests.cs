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

        public List<StandingTeeTime> ViewStandingTeeTimeRequests(DayOfWeek dayOfWeek)
        {
            List<StandingTeeTime> result = new List<StandingTeeTime>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand viewStandingTeeTimes = new SqlCommand("ViewStandingTeeTimeRequests", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    viewStandingTeeTimes.Parameters.AddWithValue("@dayOfWeek", dayOfWeek.ToString());

                    connection.Open();
                    using (SqlDataReader reader = viewStandingTeeTimes.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                StandingTeeTime standingTeeTime = new StandingTeeTime();
                                standingTeeTime.StartDate = reader["Start Date"] is DBNull ? null : (DateTime?)DateTime.Parse(reader["Start Date"].ToString());
                                standingTeeTime.EndDate = reader["End Date"] is DBNull ? null : (DateTime?)DateTime.Parse(reader["End Date"].ToString());
                                standingTeeTime.RequestedTime = DateTime.Parse(reader["Requested Time"].ToString());

                                if (!(standingTeeTime.StartDate is null) && result.LastOrDefault()?.RequestedTime == standingTeeTime.RequestedTime)
                                    standingTeeTime = result.Last();
                                if (standingTeeTime.Members is null && !(standingTeeTime.StartDate is null))
                                    standingTeeTime.Members = new List<string>();
                                if (!(standingTeeTime.Members is null) && !(standingTeeTime.StartDate is null))
                                    standingTeeTime.Members.Add(reader["Member Name"].ToString());
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
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand requestStandingTeeTime = new SqlCommand("requestStandingTeeTime", connection) {CommandType = CommandType.StoredProcedure })
                {
                    requestStandingTeeTime.Parameters.AddWithValue("@startDate", requestedStandingTeeTime.StartDate);
                    requestStandingTeeTime.Parameters.AddWithValue("@endDate", requestedStandingTeeTime.EndDate);
                    requestStandingTeeTime.Parameters.AddWithValue("@requestedTime", requestedStandingTeeTime.RequestedTime);
                    DataTable memberNumbers = new DataTable();
                    memberNumbers.Columns.Add("ID");
                    foreach (var item in requestedStandingTeeTime.Members)
                    {
                        memberNumbers.Rows.Add(item);
                    }
                    requestStandingTeeTime.Parameters.AddWithValue("@memberNumbers", memberNumbers);
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
    }
}
