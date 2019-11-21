using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalServices
{
    public class StandingTeeTime
    {
        public DateTime RequestedTime { get; set; }

        private DateTime? startDate;

        public DateTime? StartDate
        {
            get { return startDate?.Date; }
            set { startDate = value?.Date; }
        }
        private DateTime? endDate;

        public DateTime? EndDate
        {
            get { return endDate?.Date; }
            set { endDate = value?.Date; }
        }
        public List<string> Members { get; set; }

        public DayOfWeek? DayOfWeek { get { return startDate?.DayOfWeek; } }

    }
}
