using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using Microsoft.Data.SqlClient;
using AppTest.InModels;
using System.Text;


namespace AppTest.Controllers
{
    [Route("api/Group")]
    [ApiController]
    public class BuildGroupController : ControllerBase
    {
        private readonly UserContext _context;

        public BuildGroupController(UserContext context)
        {
            _context = context;
        }

        // POST: api/AddGroup
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("new_build")]
        public async Task<ActionResult> PostGroupInfo(AddGroupFromApp groupInfo)
        {
            Console.WriteLine("build a group");
            string invitedCode = null;
            int userId = -1;
            int groupId = -1;
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
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
                                return CreatedAtAction("PostGroupInfo", new { errorcode = -402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("PostGroupInfo", new { errorcode = -402, msg = "can't find user" });
                }

                //2.確認這個group有沒有人創了
                string strCheckName = "SELECT [GroupName] FROM [GroupInfo] WHERE [GroupName]=@GroupName";
                using (SqlCommand cmdCheckName = new SqlCommand(strCheckName, sqlConnection))
                {
                    cmdCheckName.Parameters.AddWithValue("@GroupName", groupInfo.GroupName);
                    using (SqlDataReader readName = cmdCheckName.ExecuteReader())
                    {
                        if (readName.HasRows)
                        {
                            if (readName.Read())
                            {
                                readName.Close();
                                sqlConnection.Close();
                                return CreatedAtAction("PostGroupInfo", new { errorcode = -400, msg = "group already exist" });
                            }
                        }
                    }

                }
                //3.可以創(你是管理員，會回傳給你邀請碼)
                string addIntoSql = " INSERT INTO [GroupInfo]([GroupName],[Age],[WinTime],[LoseTime],[InvitedCode],[GroupIntro]) VALUES(@name, @age, @win, @lose, @invite,@groupIntro) (SELECT [GroupId] FROM [GroupInfo] WHERE [GroupName]=@GroupName) ";

                using (SqlCommand cmdAddInfo = new SqlCommand(addIntoSql, sqlConnection))
                {
                    invitedCode = makeInvitedCode(groupInfo.GroupName);
                    cmdAddInfo.Parameters.AddWithValue("@name", groupInfo.GroupName);
                    cmdAddInfo.Parameters.AddWithValue("@age", GetGroupAge(groupInfo.Age));
                    cmdAddInfo.Parameters.AddWithValue("@win", 0);
                    cmdAddInfo.Parameters.AddWithValue("@lose", 0);
                    cmdAddInfo.Parameters.AddWithValue("@invite", invitedCode);
                    if(groupInfo.GroupIntro == null)
                    {
                        groupInfo.GroupIntro = "no groupIntro";
                    }
                    cmdAddInfo.Parameters.AddWithValue("@groupIntro", groupInfo.GroupIntro);
                    cmdAddInfo.Parameters.AddWithValue("@GroupName", groupInfo.GroupName);
                    //cmdAddInfo.ExecuteNonQuery();//要加才會執行命令阿-->cmdAddInfo.ExecuteReader()會做
                    using (SqlDataReader readGroupId = cmdAddInfo.ExecuteReader())
                    {
                        if (readGroupId.HasRows)
                        {
                            if (readGroupId.Read())
                            {
                                groupId = (int)readGroupId[0];
                            }
                        }
                    }
                }

                string addId = " INSERT INTO [Group_User]([GroupId],[UserId],[IsManager]) VALUES(@group, @user, @manager)";

                using (SqlCommand cmdAddId = new SqlCommand(addId, sqlConnection))
                {

                    cmdAddId.Parameters.AddWithValue("@group", groupId);
                    cmdAddId.Parameters.AddWithValue("@user", userId);
                    cmdAddId.Parameters.AddWithValue("@manager", 1);

                    cmdAddId.ExecuteNonQuery();//要加才會執行命令阿
                }

            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostGroupInfo", new { errorcode = -1, msg = "group build success", InvitedCode = invitedCode });
        }

        private int GetGroupAge(string fromApp)
        {
            switch (fromApp)
            {
                case "7~12":
                    return 1;
                case "13~16":
                    return 2;
                case "16~19":
                    return 3;
                case "19~22":
                    return 4;
                case "22~24":
                    return 5;
                case "25~30":
                    return 6;
                case "30~35":
                    return 7;
                case "35~40":
                    return 8;
                case "40~45":
                    return 9;
                case "45~50":
                    return 10;
                case "50~55":
                    return 11;
                case "55~60":
                    return 12;
                case "60以上":
                    return 13;
            }
            return -1;
        }

        private string makeInvitedCode(string GroupName)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            int ascii = Convert.ToInt32(GroupName[0]);
            return ascii + builder.ToString();
        }
        //private int CheckToken(SqlConnection sqlConnection, string token)
        //{
        //    int userId = -1;
        //    //1.先確認Token並把UserId抓下來
        //    string getUserId = @"(SELECT UserId
        //                      FROM[sportteam].[dbo].[UserInfo]
        //                        WHERE[Token] =@token 
        //                        AND[Email] = @email))";
        //    Console.WriteLine("11");
        //    using (SqlCommand cmd = new SqlCommand(getUserId, sqlConnection))
        //    {
        //        cmd.Parameters.AddWithValue("@token", token);
        //        string[] account = token.Split("/");
        //        cmd.Parameters.AddWithValue("@email", account[0]);
        //        Console.WriteLine("22");
        //        using (SqlDataReader readUserId =cmd.ExecuteReader())
        //        {
        //            if (readUserId.HasRows)
        //            {
        //                if (readUserId.Read())
        //                {
        //                    userId = (int)readUserId[0];
        //                    Console.WriteLine("getUserId" + userId);
        //                }

        //            }
        //        }
        //    }

        //    return userId;
        //}


      
    }
}
