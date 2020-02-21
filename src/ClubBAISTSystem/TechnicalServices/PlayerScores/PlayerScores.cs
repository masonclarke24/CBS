using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace TechnicalServices.PlayerScores
{
    public class PlayerScores
    {
        private readonly string email;
        private readonly string connectionString;
        public PlayerScores(string connectionString, string email)
        {
            this.connectionString = connectionString;
            this.email = email;
        }

        public bool RecordScores(ScoreCard providedScores, out string message)
        {
            bool confirmation;
            message = string.Empty;
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("RecordScores", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@course", providedScores.Course);
                    command.Parameters.AddWithValue("@rating", providedScores.Rating);
                    command.Parameters.AddWithValue("@slope", providedScores.Slope);
                    command.Parameters.AddWithValue("@date", providedScores.Date.ToString("yyyy-MM-dd HH:mm"));
                    command.Parameters.AddWithValue("@email", providedScores.Email);

                    DataTable holeByHoleScores = new DataTable();
                    holeByHoleScores.Columns.Add("HoleScore");

                    foreach (var score in providedScores.HoleByHoleScore)
                    {
                        holeByHoleScores.Rows.Add(score);
                    }

                    command.Parameters.AddWithValue("@holeByHoleScores", holeByHoleScores);
                    SqlParameter sqlMessage = new SqlParameter("@message", string.Empty) { Direction = ParameterDirection.Output, Size=128, SqlDbType = SqlDbType.VarChar };
                    command.Parameters.Add(sqlMessage);
                    command.Parameters.Add(new SqlParameter("@returnCode", -1) { Direction = ParameterDirection.ReturnValue });

                    connection.Open();

                    try
                    {
                        command.ExecuteNonQuery();
                        confirmation = command.Parameters[command.Parameters.Count - 1].Value.ToString() == "0";
                    }
                    catch (SqlException ex)
                    {
                        confirmation = false;
                        message = ex.Message;
                    }
                }
            }
            if (confirmation)
                UpdateHandicapReport(out message);
            return confirmation;
        }

        public HandicapReport GetHandicapReport(DateTime requestedMonth)
        {
            HandicapReport foundHandicapReport = null;
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("GetHandicapReport", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@month", requestedMonth.ToString("MMMM"));
                    command.Parameters.AddWithValue("@year", requestedMonth.Year);

                    connection.Open();

                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            string memberName = default;
                            DateTime lastUpdated = default;
                            double handicapFactor = default;
                            double average = default;
                            double bestofTenAverage = default;

                            while (reader.Read())
                            {
                                memberName = reader["MemberName"].ToString();
                                handicapFactor = double.Parse(reader["HandicapFactor"].ToString());
                                average = double.Parse(reader["Average"].ToString());
                                bestofTenAverage = double.Parse(reader["BestOfTenAverage"].ToString());
                                lastUpdated = (DateTime)reader["LastUpdated"]; 
                            }
                            reader.NextResult();
                            List<ScoreCard> scoreCards = new List<ScoreCard>();
                            while (reader.Read())
                            {
                                scoreCards.Add(new ScoreCard((string)reader["Course"], double.Parse(reader["Rating"].ToString()), double.Parse(reader["Slope"].ToString()), (DateTime)reader["Date"], email, new List<int>()));
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {                                
                                scoreCards.Find(s => s.Date == (DateTime)reader["Date"]).HoleByHoleScore.Add((int)reader["HoleScore"]);
                            }

                            foundHandicapReport = new HandicapReport(memberName, email, lastUpdated, handicapFactor, average, bestofTenAverage, scoreCards.ToArray());
                        }
                    }
                }
            }

            return foundHandicapReport;
        }

        public List<HandicapReport> GetAllHandicapReports()
        {
            List<HandicapReport> allHandicapReports = null;
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("GetAllHandicapReports", connection) { CommandType = CommandType.StoredProcedure })
                {
                    connection.Open();
                    
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            allHandicapReports = new List<HandicapReport>();
                            while (reader.Read())
                            {
                                allHandicapReports.Add(new HandicapReport(reader["MemberName"].ToString(), reader["Email"].ToString(), (DateTime)reader["LastUpdated"],
                                    double.Parse(reader["HandicapFactor"].ToString()), double.Parse(reader["Average"].ToString()), double.Parse(reader["BestOfTenAverage"].ToString()), null));
                            }
                        }
                    }
                }
            }

            return allHandicapReports;
        }
        private bool UpdateHandicapReport(out string message)
        {
            message = string.Empty;
            bool confirmation = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("UpdateHandicapReport", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@email", email);
                    SqlParameter sqlMessage = new SqlParameter("@message", SqlDbType.VarChar, 128) { Direction = ParameterDirection.Output };
                    command.Parameters.Add(sqlMessage);
                    command.Parameters.Add(new SqlParameter("@returnCode",-1) { Direction = ParameterDirection.ReturnValue });

                    connection.Open();

                    try
                    {
                        command.ExecuteNonQuery();
                        confirmation = command.Parameters[command.Parameters.Count - 1].Value.ToString() == "0";
                    }
                    catch (SqlException)
                    {
                        confirmation = false;
                        message = sqlMessage.Value.ToString();
                    }
                }
            }

            return confirmation;
        }
    }
}
