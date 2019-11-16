using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TechnicalServices
{
    public class DailyTeeSheets
    {
        public string MemberNumber { get; private set; }

        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;";
        private SortedDictionary<string, TeeTime> allTeeSheets;

        public DailyTeeSheets(string memberNumber)
        {
            
            MemberNumber = memberNumber;
            allTeeSheets = new SortedDictionary<string, TeeTime>();

        }

        public DailyTeeSheet FindDailyTeeSheet(DateTime date)
        {
            var minutes = new string[] { "00", "07", "15", "22", "30", "37", "45", "52" };
            for (int i = 7; i < 20; i++)
            {
                foreach (string minute in minutes)
                {
                    allTeeSheets.Add($"{i:D2}:{minute}", new TeeTime() { Datetime = DateTime.Parse($"{date.ToShortDateString()} {i:D2}:{minute}") });
                }
            }
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand findDailyTeeSheet = new SqlCommand("FindDailyTeeSheet", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    findDailyTeeSheet.Parameters.AddWithValue("@date", date.ToShortDateString());
                    findDailyTeeSheet.Connection.Open();
                    using (SqlDataReader reader = findDailyTeeSheet.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string key = reader["Time"].ToString();
                                if (allTeeSheets.ContainsKey(key))
                                {
                                    if (allTeeSheets[key].Golfers is null)
                                        allTeeSheets[key].Golfers = new List<string>();
                                    allTeeSheets[key].Golfers.Add(reader["Member Name"].ToString());
                                }
                                else
                                {
                                    allTeeSheets.Add(key,
                                    new TeeTime()
                                    {
                                        NumberOfCarts = int.Parse((string)reader["numberOfCarts"]),
                                        Phone = (string)reader["phone"],
                                        Datetime = DateTime.Parse(key)
                                    }); ;
                                }
                            }

                        }
                    }

                }

                RestrictTeeSheetToPermissableTimes(connection);
            }
            return new DailyTeeSheet() { Date = date, TeeTimes = from kvp in allTeeSheets select kvp.Value };
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
                    reserveTeeTime.Parameters.AddWithValue("@golfers", requestedTeeTime.Golfers.ToArray());
                    SqlParameter message = new SqlParameter("@message", System.Data.SqlDbType.VarChar) { Direction = System.Data.ParameterDirection.Output, Size = 64 };
                    reserveTeeTime.Parameters.Add(message);

                    reserveTeeTime.Connection.Open();

                    confirmation = reserveTeeTime.ExecuteNonQuery() > 0;

                }
            }

            return confirmation;
        }
        private void RestrictTeeSheetToPermissableTimes(SqlConnection connection)
        {
            using (SqlCommand getPermittedTeeTimes = new SqlCommand("GetPermittedTeeTimes", connection) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                getPermittedTeeTimes.Parameters.AddWithValue("@memberNumber", MemberNumber);
                getPermittedTeeTimes.Parameters.Add(new SqlParameter("@message", System.Data.SqlDbType.VarChar, 64) { Direction = System.Data.ParameterDirection.Output });
                using (SqlDataReader reader = getPermittedTeeTimes.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            foreach (var kvp in allTeeSheets)
                            {
                                if (DateTime.Parse(kvp.Key) >= DateTime.Parse(reader["TimeSpanStart"].ToString()) && DateTime.Parse(kvp.Key) <= DateTime.Parse(reader["TimeSpanEnd"].ToString()))
                                    kvp.Value.Reservable = true;
                            }
                        }
                    }
                }

            }
        }
    }
}
