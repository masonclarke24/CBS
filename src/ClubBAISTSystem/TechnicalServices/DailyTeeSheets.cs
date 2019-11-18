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
        public string MemberNumber { get; private set; }

        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;";
        private List<TeeTime> teeTimes = new List<TeeTime>();

        public DailyTeeSheets(string memberNumber)
        {
            MemberNumber = memberNumber;
        }

        public DailyTeeSheet FindDailyTeeSheet(DateTime date)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand findDailyTeeSheet = new SqlCommand("FindDailyTeeSheet", connection) { CommandType = CommandType.StoredProcedure })
                {
                    findDailyTeeSheet.Parameters.AddWithValue("@date", date.ToShortDateString());
                    findDailyTeeSheet.Connection.Open();
                    using (SqlDataReader reader = findDailyTeeSheet.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                teeTimes.Add(new TeeTime() { Datetime = DateTime.Parse($"{date.ToShortDateString()} {reader["Time"]}"), Reservable = true });
                                if (reader["Member Name"] is DBNull) continue;

                                var teeTime = teeTimes.Find(t => t.Datetime.ToShortTimeString() == reader["Time"].ToString());
                                if (teeTime.Golfers is null)
                                    teeTime.Golfers = new List<string>();
                                teeTime.Golfers.Add(reader["Member Name"].ToString());
                            }

                        }
                    }

                }

                RestrictTeeSheetToPermissableTimes(connection, date);
            }
            return new DailyTeeSheet() { Date = date, TeeTimes = teeTimes };
        }

        public bool ReserveTeeTime(TeeTime requestedTeeTime)
        {
            bool confirmation = false;
            if (!requestedTeeTime.Reservable) return false;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
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
                    SqlParameter message = new SqlParameter("@message", SqlDbType.VarChar) { Direction = ParameterDirection.Output, Size = 64 };
                    reserveTeeTime.Parameters.Add(message);

                    reserveTeeTime.Connection.Open();


                    try
                    {
                        confirmation = reserveTeeTime.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        confirmation = false;
                    }

                }
            }

            return confirmation;
        }
        private void RestrictTeeSheetToPermissableTimes(SqlConnection connection, DateTime date)
        {
            using (SqlCommand getPermittedTeeTimes = new SqlCommand("GetPermittedTeeTimes", connection) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                getPermittedTeeTimes.Parameters.AddWithValue("@memberNumber", MemberNumber);
                getPermittedTeeTimes.Parameters.AddWithValue("@dayOfWeek", date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ? "Weekend" : "Weekday");
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

                teeTimes.ForEach(t => { if (!permissableTimes.ContainsKey(t.Datetime.ToString("HH:mm:ss"))) t.Reservable = false; });

            }
        }
    }
}
