using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CBS
{
    public class DateController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public static IActionResult VerifyDate(DateTime newDate)
        {
            using (var controller = new DateController())
            {
                if (newDate.Ticks == 0)
                    return controller.Json("Supplied date is invalid");
                if (newDate.Date == DateTime.Today.Date)
                    return controller.Json("Cannot reserve tee time for today");
                if ((DateTime.Today.AddDays(7) - newDate).TotalDays < 0)
                    return controller.Json($"Selected day must not be beyond {DateTime.Today.AddDays(7).ToLongDateString()}");
                if ((DateTime.Today - newDate).TotalDays > 0)
                    return controller.Json("Selected day cannot be in the past");
                return controller.Json(true);
            }
        }
    }
}
