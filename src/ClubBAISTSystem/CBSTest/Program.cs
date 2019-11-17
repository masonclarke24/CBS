using System;
using TechnicalServices;
namespace CBSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //DailyTeeSheets d = new DailyTeeSheets("1029384756");
            //var result = d.FindDailyTeeSheet(DateTime.Parse("April 22, 2020"));

            var d = new DailyTeeSheets("1029384756");

            bool result = d.ReserveTeeTime(new TeeTime()
            {
                Datetime = DateTime.Parse("April 22, 2020 10:38"),
                NumberOfCarts = 2,
                Phone = "7804561234",
                Reservable = true,
                Golfers = new System.Collections.Generic.List<string>()
            {
                "1234567890","1029384756","1092837465"
            }
            });
        }
    }
}
