using System;

namespace TechnicalServices.Memberships
{
    public class Transaction
    {
        public DateTime? BookedDate { get; internal set; }
        public DateTime TransactionDate { get; internal set; }
        public double Amount { get; internal set; }
        public string Description { get; internal set; }
        public DateTime DueDate { get; internal set; }
    }
}