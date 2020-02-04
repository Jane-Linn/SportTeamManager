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
    [Route("api/Member")]
    [ApiController]
    public class SendRequestController : ControllerBase
    {
        private readonly UserContext _context;

        public SendRequestController(UserContext context)
        {
            _context = context;
        }
        /*
         1/27已防(防連點的感覺)(key word:sql insert if not exist)
        //******未防沒有groupId的狀況
        //******未防已經有申請或是已經是成員的狀況
        */

        // POST: api/SendRequest
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("send_application")]
        public ActionResult PostGroup_User(SendApplicationModel applicationModel)
        {
            Console.WriteLine("user send request to group");
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                int userId = -1;
                sqlConnection.Open();
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", applicationModel.Token);
                    string[] account = applicationModel.Token.Split("/");
                    cmdReadUserId.Parameters.AddWithValue("@ACCOUNT", account[0]);
                    using (SqlDataReader readUserId = cmdReadUserId.ExecuteReader())
                    {
                        if (readUserId.HasRows)
                        {
                            if (readUserId.Read())
                            {
                                userId = (int)readUserId[0];
                            }

                        }
                    }
                }
                Console.WriteLine("getUserId" + userId);
                if (userId == -1)
                {
                    return CreatedAtAction("PostGroup_User", new { errorcode = -401, msg = "can't find user" });
                }


                string strAddNotify = @"
                                        BEGIN
                                           IF NOT EXISTS(SELECT* FROM Group_User
                                                           WHERE UserId = @requestUser
                                                           AND GroupId = @groupId)
                                           BEGIN
                                               INSERT INTO Group_User(UserId, GroupId, IsManager, Accepted) VALUES(@requestUser, @groupId, 0, 0)
                                           END
                                        END ";
                using (SqlCommand cmdInsert = new SqlCommand(strAddNotify, sqlConnection))
                {
                    cmdInsert.Parameters.AddWithValue("@requestUser", userId);
                    cmdInsert.Parameters.AddWithValue("@groupId", applicationModel.GroupId);
                    if (cmdInsert.ExecuteNonQuery() == -1)
                    {
                        return CreatedAtAction("PostGroup_User", new { errorcode = 403, msg = "alreadySend" });
                    }
                }
              
            }

            return CreatedAtAction("PostGroup_User", new { errorcode = -1, msg = "send msg success" });
        }

    }
}
