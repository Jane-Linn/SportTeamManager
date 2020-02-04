using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using Microsoft.Data.SqlClient;
using AppTest.InModels;

namespace AppTest.Controllers
{
    [Route("api/Manager")]
    [ApiController]
    public class ManagerEmailController : ControllerBase
    {
        private readonly UserContext _context;

        public ManagerEmailController(UserContext context)
        {
            _context = context;
        }


        // POST: api/ManagerEmail
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("email")]
        public ActionResult GetManagerEmail(SendToken token)
        {
            Console.WriteLine("show manager email");
            List<ManagerEmail> managerEmails = new List<ManagerEmail>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string + " ;MultipleActiveResultSets = true"))
            {
                int userId = -1;
                sqlConnection.Open();
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", token.Token);
                    string[] account = token.Token.Split("/");
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
                Console.WriteLine("managerUserId" + userId);
                if (userId == -1)
                {
                    return CreatedAtAction("GetManagerEmail", new { errorcode = 401, msg = "can't find user" });
                }

                string strSearchNotify = @" SELECT Group_User.Id,Group_User.UserId, UserName, GroupName From [Group_User],[UserInfo], [GroupInfo]
                                                Where UserInfo.UserId=Group_User.UserId 
                                                AND GroupInfo.GroupId=Group_User.GroupId 
                                                AND Group_User.GroupId IN (SELECT GroupId From [Group_User] Where UserId=@userId AND IsManager=1) AND Accepted=0";
                using (SqlCommand cmd = new SqlCommand(strSearchNotify, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader readNotify = cmd.ExecuteReader())
                    {
                        if (readNotify.HasRows)
                        {
                            while (readNotify.Read())
                            {
                                ManagerEmail newObj = new ManagerEmail();
                                newObj.NotifyId = readNotify.GetInt32(0);
                                newObj.RequestId = readNotify.GetInt32(1);
                                newObj.RequestName = readNotify.GetString(2);
                                newObj.GroupName = readNotify.GetString(3);
                                managerEmails.Add(newObj);
                            }
                            //Console.WriteLine("3");
                            //readGroup.NextResult();
                            //Console.WriteLine("4");
                            return CreatedAtAction("GetManagerEmail", new { errorcode = -1, msg = "success get email", mail = managerEmails });
                        }
                        else
                        {
                            return CreatedAtAction("GetManagerEmail", new { errorcode = 402, msg = "you dont have any email" });
                        }

                    }

                }

            }

        }
    }
}
