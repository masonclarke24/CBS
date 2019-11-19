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

        public IActionResult VerifyDate(DateTime date)
        {
            if (date.Ticks == 0)
                return Json("Date is in the wrong format");
            if (date.Date == DateTime.Today.Date)
                return Json("Cannot reserve tee time for today");
            if ((DateTime.Today.AddDays(7) - date).TotalDays < 0)
                return Json($"Selected day must not be beyond {DateTime.Today.AddDays(7).ToLongDateString()}");
            if ((DateTime.Today - date).TotalDays > 0)
                return Json("Selected day cannot be in the past");
            return Json(true);
        }
    }
}
