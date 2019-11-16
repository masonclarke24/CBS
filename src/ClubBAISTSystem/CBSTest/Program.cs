using System;
using TechnicalServices;
namespace CBSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DailyTeeSheets d = new DailyTeeSheets("1029384756");
            var result = d.FindDailyTeeSheet(DateTime.Parse("November 19,2019"));

        }
    }
}
