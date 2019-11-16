using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices
{
    class DailyTeeSheet
    {
        public DateTime Date { get; set; }
        public TeeTime[] TeeTimes { get; set; }
    }
}
