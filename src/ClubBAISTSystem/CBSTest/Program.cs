using System;
using CBSClasses;
namespace CBSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StandingTeeTimeRequests sr = new StandingTeeTimeRequests("Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;");
            //var result = sr.ViewStandingTeeTimeRequests(DayOfWeek.Tuesday);
        }
    }
}
