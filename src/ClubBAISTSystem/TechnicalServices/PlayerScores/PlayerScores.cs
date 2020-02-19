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
