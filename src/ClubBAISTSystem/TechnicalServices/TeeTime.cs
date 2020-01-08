using System;
using System.Collections.Generic;
using System.Linq;

namespace TechnicalServices
{
    public class TeeTime
    {
        public DateTime Datetime { get; set; }
        public int NumberOfCarts { get; set; }
        public List<string> Golfers { get; set; }
        public string Phone { get; set; }
        public bool Reservable { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is TeeTime teeTime)) return false;

            return teeTime.Datetime == Datetime && (teeTime.Golfers?.SequenceEqual(Golfers) ?? true) && teeTime.Phone == Phone;
        }

        public override int GetHashCode()
        {
            return Datetime.Minute * Datetime.Month;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
