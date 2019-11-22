using System;
using System.Collections.Generic;
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

                                if (!(standingTeeTime.StartDate is null) && result.Last().StartDate == standingTeeTime.StartDate)
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
    }
}
