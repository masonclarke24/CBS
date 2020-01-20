using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CBS.Areas.Identity.Pages.Account.Manage
{
    public class UpdateTeeTimeModel : PageModel
    {
        public void OnGet()
        {
            if(Request.Query.TryGetValue("teeTimeTime", out Microsoft.Extensions.Primitives.StringValues teeTimeTime))
            {
                
            }
            else
            {
                HttpContext.Response.Redirect("/Errors/Error404");
            }
        }
    }
}
