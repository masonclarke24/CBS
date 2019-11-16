using System;
using System.Collections.Generic;

namespace TechnicalServices
{
    public class TeeTime
    {
        public DateTime Datetime { get; set; }
        public int NumberOfCarts { get; set; }
        public List<string> Golfers { get; set; }
        public string Phone { get; set; }
        public bool Reservable { get; set; }
    }
}
