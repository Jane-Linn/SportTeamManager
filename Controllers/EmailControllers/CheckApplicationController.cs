using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using AppTest.InModels;
using Microsoft.Data.SqlClient;

namespace AppTest.Controllers
{
    [Route("api/Manager")]
    [ApiController]
    public class CheckApplicationController : ControllerBase
    {
        private readonly UserContext _context;

        public CheckApplicationController(UserContext context)
        {
            _context = context;
        }
        //********未做確認他是manager(因為正常狀況你是manager才會出現這個通知
        //有確認這個請求還在不在(有沒有被收回 或 其他管理員已確認)

        // POST: api/CheckApplication
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("check_email")]
        public ActionResult CheckRequest(CheckApplication checkApplication)
        {
            int acceptedStatus = -1;
            switch (checkApplication.Status)
            {
                case "accept":
                    acceptedStatus = 2;
                    //改notify & group_user
                    break;
                case "reject":
                    acceptedStatus = 3;
                    //改notify
                    break;
            }

            Console.WriteLine("check manager email" + acceptedStatus);
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strIdExist = @"SELECT Id FROM Group_User Where Id = @notifyId";
                using (SqlCommand cmd = new SqlCommand(strIdExist, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@notifyId", checkApplication.NotifyId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows && !reader.Read())
                        {
                            return CreatedAtAction("CheckRequest", new { errorcode = 401, msg = "done by other manager" });
                        }
                    }
                }
                //同意別人加入球隊 隨機給予背號??????
                string strCheck = "UPDATE Group_User SET Accepted=@status,JoinGroupDate=@joinDate WHERE Id=@notifyId";
                using (SqlCommand sqlCommand = new SqlCommand(strCheck, sqlConnection))
                {

                    sqlCommand.Parameters.AddWithValue("@status", acceptedStatus);
                    sqlCommand.Parameters.AddWithValue("@notifyId", checkApplication.NotifyId);
                    sqlCommand.Parameters.AddWithValue("@joinDate", DateTime.Now);
                    sqlCommand.ExecuteNonQuery();
                    return CreatedAtAction("CheckRequest", new { errorcode = -1, msg = "already process" });


                }
            }
        }

    }

}