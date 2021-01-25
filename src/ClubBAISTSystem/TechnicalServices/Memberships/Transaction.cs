using Newtonsoft.Json;
using System;

namespace TechnicalServices.Memberships
{
    public class Transaction
    {
        [JsonProperty]
        public DateTime? BookedDate { get; internal set; }
        [JsonProperty]
        public DateTime TransactionDate { get; internal set; }
        [JsonProperty]
        public double Amount { get; internal set; }
        [JsonProperty]
        public string Description { get; internal set; }
        [JsonProperty]
        public DateTime DueDate { get; internal set; }
    }
}