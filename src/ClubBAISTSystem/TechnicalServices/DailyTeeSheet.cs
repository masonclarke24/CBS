using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices
{
    public class DailyTeeSheet
    {
        public DateTime Date { get; set; }
        public List<TeeTime> TeeTimes { get; set; }
    }
}
