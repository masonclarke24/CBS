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

                                TeeTime newTeeTime = CreateTeeTimeFromReader(date, reader);

                                teeTimes.Push(newTeeTime);
                            }

                        }
                    }

                }

                GetPermittedTeeTimes(connection, date);
            }
            return new DailyTeeSheet() { Date = date, TeeTimes = teeTimes.Reverse().ToList() };
        }

        public List<TeeTime> FindReservedTeeTimes()
        {
            Stack<TeeTime> golferTeeTimes = new Stack<TeeTime>();
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand findReservedTeeTimes = new SqlCommand("FindReservedTeeTimes", connection) { CommandType = CommandType.StoredProcedure })
                {
                    findReservedTeeTimes.Parameters.AddWithValue("@userID", MemberNumber);

                    connection.Open();

                    using(SqlDataReader reader = findReservedTeeTimes.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            do
                            {
                                while (reader.Read())
                                {
                                    if (golferTeeTimes.Any())
                                    {
                                        if (DateTime.Parse($"{reader["Date"]}").Add(DateTime.Parse($"{reader["Time"]}").TimeOfDay)
                                                                    == golferTeeTimes.Peek().Datetime)
                                        {
                                            golferTeeTimes.Peek().Golfers.Add(reader["Member Name"].ToString());
                                            continue;
                                        }
                                    }

                                    TeeTime reservedTeeTime = CreateTeeTimeFromReader(DateTime.Parse($"{reader["Date"]}"), reader);

                                    golferTeeTimes.Push(reservedTeeTime);
                                }
                            } while (reader.NextResult());
                        }
                    }
                }
            }

            return golferTeeTimes.ToList();
        }

        public bool CancelTeeTime(DateTime teeTimeTime)
        {
            bool confirmation = false;

            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand cancelTeeTime = new SqlCommand("CancelTeeTime", connection) { CommandType = CommandType.StoredProcedure })
                {
                    cancelTeeTime.Parameters.AddWithValue("@date", teeTimeTime.ToShortDateString());
                    cancelTeeTime.Parameters.AddWithValue("@time", teeTimeTime.ToLongTimeString());
                    cancelTeeTime.Parameters.AddWithValue("@userID", MemberNumber);

                    cancelTeeTime.Parameters.Add(new SqlParameter("@ReturnCode", -1) { Direction = ParameterDirection.ReturnValue });

                    connection.Open();

                    try
                    {
                        cancelTeeTime.ExecuteNonQuery();
                        confirmation = (int)cancelTeeTime.Parameters[cancelTeeTime.Parameters.Count - 1].Value == 0;
                    }
                    catch (SqlException ex)
                    {
                        confirmation = false;
                    }
                }
            }

            return confirmation;
        }

        public bool UpdateTeeTime(DateTime teeTimeTime, string newPhone, int newNumberOfCarts, List<string> newGolfers, out string message)
        {
            bool confirmation = false;
            message = "";
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand updateTeeTime = new SqlCommand("UpdateTeeTime", connection) { CommandType = CommandType.StoredProcedure })
                {
                    updateTeeTime.Parameters.AddWithValue("@date", teeTimeTime.ToShortDateString());
                    updateTeeTime.Parameters.AddWithValue("@time", teeTimeTime.ToLongTimeString());
                    updateTeeTime.Parameters.AddWithValue("phone", newPhone);
                    updateTeeTime.Parameters.AddWithValue("@numberOfCarts", newNumberOfCarts);

                    DataTable golfers = new DataTable("@newGolfers");
                    golfers.Columns.Add("MemberNumber");
                    foreach (string golfer in newGolfers)
                    {
                        golfers.Rows.Add(golfer);
                    }

                    updateTeeTime.Parameters.AddWithValue("@newGolfers", golfers);

                    updateTeeTime.Parameters.Add(new SqlParameter("@ReturnCode", -1) { Direction = ParameterDirection.ReturnValue });

                    connection.Open();

                    try
                    {
                        updateTeeTime.ExecuteNonQuery();
                        confirmation = (int)updateTeeTime.Parameters[updateTeeTime.Parameters.Count - 1].Value == 0;
                    }
                    catch (SqlException ex)
                    {
                        confirmation = false;
                        message = ex.Message;
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

        private static TeeTime CreateTeeTimeFromReader(DateTime date, SqlDataReader reader)
        {
            return new TeeTime()
            {
                Datetime = DateTime.Parse($"{date.Date.ToLongDateString()} {reader["Time"]}"),
                NumberOfCarts = reader["NumberOfCarts"] is DBNull ? default : int.Parse($"{reader["NumberOfCarts"]}"),
                Phone = reader["Phone"] is DBNull ? default : $"{reader["Phone"]}",
                Golfers = reader["Member Name"] is DBNull ? default : new List<string> { $"{reader["Member Name"]}" },
                Reservable = true
            };
        }
    }
}
