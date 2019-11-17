using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CBS
{
    public class MembersController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CheckMemberNumber([Bind(Prefix = "Input.MemberNumber")] string MemberNumber)
        {
            using (SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;"))
            {
                using (SqlCommand checkMemberNumber = new SqlCommand("MemberExists", connection) { CommandType = System.Data.CommandType.StoredProcedure})
                {
                    checkMemberNumber.Parameters.AddWithValue("@memberNumber", MemberNumber);
                    //SqlParameter returnCode = new SqlParameter("@returnCode", -1) { Direction = System.Data.ParameterDirection.ReturnValue};
                    connection.Open();
                    int returnCode = int.Parse(checkMemberNumber.ExecuteScalar().ToString());
                    if (returnCode == 0)
                        return Json(true);
                }
            }

            return Json("Supplied member number does not exist");
            
        }
    }
}
