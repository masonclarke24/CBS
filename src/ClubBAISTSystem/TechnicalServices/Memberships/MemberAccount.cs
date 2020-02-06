using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Linq;

namespace TechnicalServices.Memberships
{
    public class MemberAccount
    {
        public string Name { get; private set; }
        public string MembershipLevel { get; private set; }
        public MembershipType? MembershipType { get; private set; }
        public double Balance { get; private set; }
        private List<Transaction> transactions;
        public List<Transaction> Transactions { get { return new List<Transaction>(transactions); } }
        private readonly string email;
        private readonly string connectionString;
        public MemberAccount(string email, string connectionString)
        {
            this.email = email;
            this.connectionString = connectionString;
        }

        public void GetAccountDetails(DateTime fromDate, DateTime toDate)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand command = new SqlCommand("GetAccountDetail", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@fromDate", fromDate);
                    command.Parameters.AddWithValue("@toDate", toDate);

                    connection.Open();

                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Name = reader["Name"] is DBNull ? null : reader["Name"].ToString();
                                MembershipLevel = reader["MembershipLevel"] is DBNull ? null : reader["MembershipLevel"].ToString();
                                Balance = reader["Balance"] is DBNull ? double.NaN : double.Parse(reader["Balance"].ToString());
                                MembershipType = (reader["MembershipType"] is DBNull ? null : reader["MembershipType"].ToString().ToUpper()) switch
                                {
                                    "ASSOCIATE" => TechnicalServices.MembershipType.Associate,
                                    "SHAREHOLDER" => TechnicalServices.MembershipType.Shareholder,
                                    _ => null,
                                };
                            }
                        }
                        reader.NextResult();
                        if (reader.HasRows)
                        {
                            transactions = new List<Transaction>();
                            while (reader.Read())
                            {
                                Transaction transaction = new Transaction
                                {
                                    TransactionDate = (DateTime)reader["TransactionDate"],
                                    BookedDate = reader["BookedDate"] is DBNull ? null : (DateTime?)reader["BookedDate"],
                                    Amount = double.Parse(reader["Amount"].ToString()),
                                    Description = reader["Description"].ToString(),
                                    DueDate = (DateTime)reader["DueDate"]
                                };
                                transactions.Add(transaction);
                            }
                        }
                    }
                }
            }
        }

        public void FilterAccountDetails(string description)
        {
            transactions = (from tran in transactions where tran.Description.Contains(description) select tran).ToList();
        }
    }
}
