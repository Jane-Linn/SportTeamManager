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
    [Route("api/Group")]
    [ApiController]
    public class GroupListController : ControllerBase
    {
        private readonly UserContext _context;

        public GroupListController(UserContext context)
        {
            _context = context;
        }
        // POST: api/Group/all_i_have
        [HttpPost("all_i_have")]
        public ActionResult GetMyGroup(AddGroupFromApp groupInfo)
        {
            Console.WriteLine("show all of the group i have");
            List<ShowGroup> showGroups = new List<ShowGroup>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                //1.先確認Token並把UserId抓下來
                //int userId = CheckToken(sqlConnection, groupInfo.Token);
                int userId = -1;
                //1.先確認Token並把UserId抓下來
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", groupInfo.Token);
                    string[] account = groupInfo.Token.Split("/");
                    cmdReadUserId.Parameters.AddWithValue("@ACCOUNT", account[0]);
                    using (SqlDataReader readUserId = cmdReadUserId.ExecuteReader())
                    {
                        if (readUserId.HasRows)
                        {
                            if (readUserId.Read())
                            {
                                userId = (int)readUserId[0];
                            }
                            else
                            {
                                return CreatedAtAction("GetMyGroup", new { errorcode = -402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("GetMyGroup", new { errorcode = -402, msg = "can't find user" });
                }
                Console.WriteLine("UserId"+userId);
                //2.使用GroupId查詢List
                string getMyGroup = @"
                   SELECT [GroupId],[GroupName],[GroupIntro],[InvitedCode],[GroupPhoto]
                     FROM [GroupInfo]
                     WHERE [GroupId] IN
                       (SELECT GroupId
                          FROM [Group_User]
                          WHERE UserId =@id AND (IsManager=0 OR IsManager=1) AND (Accepted=1 OR Accepted=2))";
                using (SqlCommand cmd = new SqlCommand(getMyGroup, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (SqlDataReader readGroup = cmd.ExecuteReader())
                    {
                        if (readGroup.HasRows)
                        {
                            while (readGroup.Read())
                            {
                                ShowGroup newObj = new ShowGroup();
                                newObj.GroupId = readGroup.GetInt32(0);
                                newObj.GroupName = readGroup.GetString(1);
                                newObj.GroupIntro = readGroup.GetString(2);
                                newObj.InvitedCode = readGroup.GetString(3);
                                string photoRoute = null;
                                if (!readGroup.IsDBNull(4))
                                {
                                 photoRoute = Global.group_photo_url + readGroup.GetString(4);
                                }
                              
                                newObj.GroupPhoto = photoRoute;
                                showGroups.Add(newObj);

                            }
                            //Console.WriteLine("3");
                            //readGroup.NextResult();
                            //Console.WriteLine("4");
                        }
                        else
                        {
                            return CreatedAtAction("GetMyGroup", new { errorcode = -402, msg = "you dont have any group" });
                        }

                    }
                }
            }
            //Console.WriteLine(showGroups.ElementAt<ShowGroup>(0));
            //Console.WriteLine(showGroups.ElementAt<ShowGroup>(1));

            return CreatedAtAction("GetMyGroup", new { errorcode = -1, msg = "success get group", MyGroups = showGroups });
        }

    }
}
