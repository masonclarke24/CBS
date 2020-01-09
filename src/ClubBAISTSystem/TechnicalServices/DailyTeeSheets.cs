using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TechnicalServices
{
    public class DailyTeeSheets
    {
        public string MemberNumber { get; private set; }

        private HashSet<TeeTime> teeTimes = new HashSet<TeeTime>();
        private readonly string connectionString;

        public DailyTeeSheets(string memberNumber, string connectionString)
        {
            MemberNumber = memberNumber;
            this.connectionString = connectionString;
        }

        public DailyTeeSheet FindDailyTeeSheet(DateTime date)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand findDailyTeeSheet = new SqlCommand("FindDailyTeeSheet", connection) { CommandType = CommandType.StoredProcedure })
                {
                    findDailyTeeSheet.Parameters.AddWithValue("@date", date);
                    findDailyTeeSheet.Connection.Open();
                    using (SqlDataReader reader = findDailyTeeSheet.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                teeTimes.Add(new TeeTime() { Datetime = DateTime.Parse($"{date.ToShortDateString()} {reader["Time"]}"), Reservable = true });
                                if (reader["Member Name"] is DBNull) continue;

                                var teeTime = (from t in teeTimes where t.Datetime.ToString("HH:mm") == reader["Time"].ToString() select t).FirstOrDefault();
                                if (teeTime.Golfers is null)
                                    teeTime.Golfers = new List<string>();
                                teeTime.Golfers.Add(reader["Member Name"].ToString());
                            }

                        }
                    }

                }

                GetPermittedTeeTimes(connection, date);
            }
            return new DailyTeeSheet() { Date = date, TeeTimes = teeTimes.ToList() };
        }

        public bool ReserveTeeTime(TeeTime requestedTeeTime, out string error)
        {
            bool confirmation = false;
            error = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand reserveTeeTime = new SqlCommand("ReserveTeeTime", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    reserveTeeTime.Parameters.AddWithValue("@date", requestedTeeTime.Datetime);
                    reserveTeeTime.Parameters.AddWithValue("@time", requestedTeeTime.Datetime);
                    reserveTeeTime.Parameters.AddWithValue("@numberOfCarts", requestedTeeTime.NumberOfCarts);
                    reserveTeeTime.Parameters.AddWithValue("@phone", requestedTeeTime.Phone);
                    var golfers = new DataTable("@golfers");
                    golfers.Columns.Add("MemberNumber");
                    foreach (var item in requestedTeeTime.Golfers.ToArray())
                    {
                        golfers.Rows.Add(item);
                    }
                    
                    reserveTeeTime.Parameters.AddWithValue("@golfers", golfers);
                    SqlParameter message = new SqlParameter("@message", SqlDbType.VarChar) { Direction = ParameterDirection.Output, Size = 8000 };
                    reserveTeeTime.Parameters.Add(message);

                    reserveTeeTime.Connection.Open();

                    
                    try
                    {
                        confirmation = reserveTeeTime.ExecuteNonQuery() != 0;

                    }
                    catch (Exception ex)
                    {
                        confirmation = false;
                        error = ex.Message;
                    }

                    reserveTeeTime.Connection.Close();
                }
            }

            return confirmation;
        }


        private void GetPermittedTeeTimes(SqlConnection connection, DateTime date)
        {
            using (SqlCommand getPermittedTeeTimes = new SqlCommand("GetPermittedTeeTimes", connection) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                getPermittedTeeTimes.Parameters.AddWithValue("@memberNumber", MemberNumber);
                getPermittedTeeTimes.Parameters.AddWithValue("@dayOfWeek", (int)date.DayOfWeek + 1);
                SortedList<string, object> permissableTimes = new SortedList<string,object>();
                using (SqlDataReader reader = getPermittedTeeTimes.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            permissableTimes.Add(reader["Time"].ToString(), null);
                        }
                    }
                }
                foreach (var item in teeTimes)
                {
                    if (!permissableTimes.ContainsKey(item.Datetime.ToString("HH:mm:ss")))
                        item.Reservable = false;
                }

            }
        }
    }
}
