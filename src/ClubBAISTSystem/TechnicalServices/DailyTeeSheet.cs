using System;
using System.Collections.Generic;
using System.Text;

namespace CBSClasses
{
    public class DailyTeeSheet
    {
        public DateTime Date { get; set; }
        public List<TeeTime> TeeTimes { get; set; }
    }
}
