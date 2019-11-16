using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TechnicalServices
{
    class DailyTeeSheets
    {
        public string MemberNumber { get; private set; }

        public DailyTeeSheets(string memberNumber)
        {
            MemberNumber = memberNumber;
        }

        public DailyTeeSheet FindDailyTeeSheet(DateTime date)
        {
            using (SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;"))
            {
                using(SqlCommand findDailyTeeSheet = new SqlCommand("FindDailyTeeSheet", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    findDailyTeeSheet.Parameters.AddWithValue("@date", date.ToShortDateString());
                }
            }
            return null;
        }
    }
}
