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
    public class UserEmailController : ControllerBase
    {
        private readonly UserContext _context;

        public UserEmailController(UserContext context)
        {
            _context = context;
        }
        // POST: api/UserEmail
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("email")]
        public ActionResult GetUserEmail(SendToken token)
        {
            Console.WriteLine("show user email");
            List<UserEmail> userEmails = new List<UserEmail>();
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
                Console.WriteLine("getUserId" + userId);
                if (userId == -1)
                {
                    return CreatedAtAction("GetUserEmail", new { errorcode = 401, msg = "can't find user" });
                }
                string strSearchNotify = @"SELECT Group_User.Id, GroupName, Group_User.Accepted
                                                From [Group_User], [GroupInfo]
                                                Where GroupInfo.GroupId=Group_User.GroupId 
                                                AND  Group_User.UserId =@userId AND (Accepted = 2 OR Accepted = 3)";
                using (SqlCommand cmd = new SqlCommand(strSearchNotify, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader readNotify = cmd.ExecuteReader())
                    {
                        if (readNotify.HasRows)
                        {
                            while (readNotify.Read())
                            {
                                UserEmail newObj = new UserEmail();
                                newObj.NotifyId = readNotify.GetInt32(0);
                                newObj.GroupName = readNotify.GetString(1);
                                int acceptedStatus = readNotify.GetInt32(2);
                                if (acceptedStatus == 2)
                                {
                                    newObj.AcceptedOrNot = "has been accepted";
                                }else if(acceptedStatus == 3)
                                {
                                    newObj.AcceptedOrNot = "been rejected";
                                }
                                
                                userEmails.Add(newObj);
                            }
                            //Console.WriteLine("3");
                            //readGroup.NextResult();
                            //Console.WriteLine("4");
                            return CreatedAtAction("GetUserEmail", new { errorcode = -1, msg = "success get email", mail = userEmails });
                        }
                        else
                        {
                            return CreatedAtAction("GetUserEmail", new { errorcode = 402, msg = "you dont have any email" });
                        }

                    }

                }
            }



        }

    }
}
