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
    [Route("api/User")]
    [ApiController]
    public class CheckEmailController : ControllerBase
    {
        private readonly UserContext _context;

        public CheckEmailController(UserContext context)
        {
            _context = context;
        }
        //不管是否被加入社團都會收到回信，要確認刪除信件
        //判斷是否有此id????????????????????????????分兩次?
        // POST: api/CheckEmail
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("check_email")]
        public ActionResult CheckEmail(CheckEmailModel checkEmail)
        {
            Console.WriteLine("check user email");
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strIdExist = @"SELECT Id FROM Group_User Where Id = @notifyId";
                using(SqlCommand cmd = new SqlCommand(strIdExist, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@notifyId", checkEmail.NotifyId);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(!reader.HasRows && !reader.Read())
                        {
                            return CreatedAtAction("CheckEmail", new { errorcode = 401, msg = "can't find notifyId" });
                        }
                    }
                }
                string strCheck = @"IF (SELECT Accepted FROM Group_User Where Id = @notifyId)=2 
                                    BEGIN
                                        Update Group_User SET Accepted = 1 Where Id = @notifyId
                                    END
                                    ELSE IF (SELECT Accepted FROM Group_User Where Id = @notifyId)=3
                                    BEGIN
                                        DELETE From Group_User WHERE Id = @notifyId
                                    END ";
                using(SqlCommand cmd = new SqlCommand(strCheck, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@notifyId", checkEmail.NotifyId);
                    cmd.ExecuteNonQuery();
                    return CreatedAtAction("CheckEmail", new { errorcode= -1, msg="success check email" }) ;
                }
            }

        }

    }
}
