﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TechnicalServices
{
    public class DailyTeeSheets
    {
        private string userId;

        private Stack<TeeTime> teeTimes = new Stack<TeeTime>();
        private readonly string connectionString;

        public DailyTeeSheets(string memberNumber, string connectionString)
        {
            userId = memberNumber;
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
                                        teeTimes.Peek().Golfers.Add((reader["Member Name"].ToString(), reader["UserId"].ToString()));
                                        continue;
                                    }
                                }

                                TeeTime newTeeTime = CreateTeeTimeFromReader(date, reader);

                                teeTimes.Push(newTeeTime);
                            }

                        }
                    }

                }
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
                    findReservedTeeTimes.Parameters.AddWithValue("@userID", userId);

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
                                            golferTeeTimes.Peek().Golfers.Add((reader["Member Name"].ToString(), reader["UserId"].ToString()));
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
                    cancelTeeTime.Parameters.AddWithValue("@userID", userId);

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

        public bool UpdateTeeTime(DateTime teeTimeTime, string newPhone, int? newNumberOfCarts, List<string> newGolfers, bool? checkedIn, out string message)
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
                    updateTeeTime.Parameters.AddWithValue("@checkedIn", checkedIn);

                    DataTable golfers = new DataTable("@newGolfers");
                    golfers.Columns.Add("UserId");
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
                    reserveTeeTime.Parameters.AddWithValue("@reservedBy", requestedTeeTime.ReservedBy);
                    var golfers = new DataTable("@golfers");
                    golfers.Columns.Add("UserId");

                    foreach (var (Name, UserId) in requestedTeeTime.Golfers.ToArray())
                    {
                        golfers.Rows.Add(UserId);
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

        public IEnumerable<TeeTime> FilterDailyTeeSheet(DateTime date, string userId, IEnumerable<TeeTime> teeTimes)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand getPermittedTeeTimes = new SqlCommand("GetPermittedTeeTimes", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    getPermittedTeeTimes.Parameters.AddWithValue("@userId", userId);
                    getPermittedTeeTimes.Parameters.AddWithValue("@dayOfWeek", (int)date.DayOfWeek + 1);
                    SortedList<string, object> permissableTimes = new SortedList<string, object>();
                    connection.Open();
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

            return teeTimes;
        }

        private static TeeTime CreateTeeTimeFromReader(DateTime date, SqlDataReader reader)
        {
            return new TeeTime()
            {
                Datetime = DateTime.Parse($"{date.Date.ToLongDateString()} {reader["Time"]}"),
                NumberOfCarts = reader["NumberOfCarts"] is DBNull ? default : int.Parse($"{reader["NumberOfCarts"]}"),
                Phone = reader["Phone"] is DBNull ? default : $"{reader["Phone"]}",
                Golfers = reader["Member Name"] is DBNull ? default : new List<(string, string)> { { (reader["Member Name"].ToString(), reader["UserId"].ToString()) } },
                Reservable = true,
                ReservedBy = reader["ReservedBy"] is DBNull ? null : reader["ReservedBy"].ToString(),
                CheckedIn = reader["Checked In"] is DBNull ? default : bool.Parse(reader["Checked In"].ToString())
            };
        }
    }
}
