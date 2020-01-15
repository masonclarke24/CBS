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

        private Stack<TeeTime> teeTimes = new Stack<TeeTime>();
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
                                if (teeTimes.Any())
                                {
                                    if (DateTime.Parse($"{date.ToLongDateString()} {reader["Time"]}") == teeTimes.Peek().Datetime)
                                    {
                                        teeTimes.Peek().Golfers.Add(reader["Member Name"].ToString());
                                        continue;
                                    } 
                                }

                                TeeTime newTeeTime = new TeeTime()
                                {
                                    Datetime = DateTime.Parse($"{date.Date.ToLongDateString()} {reader["Time"]}"),
                                    NumberOfCarts = int.Parse(reader["NumberOfCarts"].ToString()),
                                    Phone = reader["Phone"].ToString(),
                                    Golfers = new List<string> { reader["Member Name"].ToString() }
                                };

                                teeTimes.Push(newTeeTime);
                            }

                        }
                    }

                }

                GetPermittedTeeTimes(connection, date);
            }
            return new DailyTeeSheet() { Date = date, TeeTimes = teeTimes.ToList() };
        }

        public List<TeeTime> FindReservedTeeTimes(string userID)
        {
            Stack<TeeTime> golferTeeTimes = new Stack<TeeTime>();
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand findReservedTeeTimes = new SqlCommand("FindReservedTeeTimes") { CommandType = CommandType.StoredProcedure })
                {
                    findReservedTeeTimes.Parameters.AddWithValue("@userID", userID);

                    connection.Open();

                    using(SqlDataReader reader = findReservedTeeTimes.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (DateTime.Parse($"{reader["Date"]} {reader["Time"]}") == golferTeeTimes.Peek().Datetime)
                                {
                                    golferTeeTimes.Peek().Golfers.Add(reader["Member Name"].ToString());
                                    continue;
                                }

                                TeeTime reservedTeeTime = new TeeTime()
                                {
                                    Datetime = DateTime.Parse($"{reader["Date"]} {reader["Time"]}"),
                                    NumberOfCarts = int.Parse(reader["NumberOfCarts"].ToString()),
                                    Phone = reader["Phone"].ToString(),
                                    Golfers = new List<string> { reader["Member Name"].ToString() }
                                };

                                golferTeeTimes.Push(reservedTeeTime); 
                            }
                        }
                    }
                }
            }

            return golferTeeTimes.ToList();
        }

        public bool CancelTeeTime(DateTime teeTimeTime, string userID)
        {
            bool confirmation = false;

            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand cancelTeeTime = new SqlCommand("CancelTeeTime", connection))
                {
                    cancelTeeTime.Parameters.AddWithValue("@date", teeTimeTime.Date);
                    cancelTeeTime.Parameters.AddWithValue("@time", teeTimeTime.ToLongTimeString());
                    cancelTeeTime.Parameters.AddWithValue("@userID", userID);

                    connection.Open();

                    try
                    {
                        confirmation = cancelTeeTime.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        confirmation = false;
                    }
                }
            }

            return confirmation;
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
