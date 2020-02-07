using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace TechnicalServices.Memberships
{
    public class MemberAccount
    {
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public string MembershipLevel { get; private set; }
        [JsonProperty]
        public MembershipType? MembershipType { get; private set; }
        [JsonProperty]
        public double Balance { get; private set; }
        [JsonProperty]
        public DateTime FromDate { get; private set; }
        [JsonProperty]
        public DateTime ToDate { get; private set; }
        [JsonProperty]
        private List<Transaction> transactions;
        [JsonProperty]
        private List<Transaction> filteredTransactions;
        public List<Transaction> Transactions { get { return filteredTransactions is null ? new List<Transaction>(transactions) : new List<Transaction>(filteredTransactions); } }
        private readonly string email;
        [JsonProperty]
        private readonly string connectionString;
        [JsonConstructor]
        public MemberAccount(string email, string connectionString)
        {
            this.email = email;
            this.connectionString = connectionString;
        }

        public void GetAccountDetails(DateTime fromDate, DateTime toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
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
            filteredTransactions = (from tran in transactions where tran.Description.ToUpper().Contains(description?.ToUpper() ?? "") select tran).ToList();
        }
    }
}
