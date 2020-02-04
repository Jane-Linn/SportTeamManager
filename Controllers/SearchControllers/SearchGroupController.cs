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
    public class SearchGroupController : ControllerBase
    {
        private readonly UserContext _context;

        public SearchGroupController(UserContext context)
        {
            _context = context;
        }
        //*****改漂亮:if(hasrow || read) 重複使用的不要用using(限制範圍)-->可以重複使用
        //1.判斷token是否正確
        //2.判斷有哪些group
        //3.判斷 是否加入了 是否申請過 (在 group_user 中比對 groupId UserId) 
        // POST: api/MemberGroup/5
        [HttpPost("search_group")]
        public async Task<ActionResult> GetGroupInfo(SearchGroup searchGroup)
        {
            int groupId = -1;
            int userId = -1;
            string status = null;
            //1.確認token
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string + " ;MultipleActiveResultSets = true"))
            {
                sqlConnection.Open();
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", searchGroup.Token);
                    string[] account = searchGroup.Token.Split("/");
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
                if (userId == -1)
                {
                    return CreatedAtAction("GetGroupInfo", new { errorcode = -401, msg = "can't find user" });
                }
                if (searchGroup.GroupName == null)
                {//2.傳入的是invitedcode
                    Console.WriteLine("search group by invited code");
                    ReturnSearchGroup resultGroup = new ReturnSearchGroup();
                    string strGetGroup = "SELECT GroupId,GroupName,GroupIntro,[GroupPhoto] FROM [GroupInfo] WHERE [InvitedCode]=@invitedCode";
                    using (SqlCommand cmd = new SqlCommand(strGetGroup, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@invitedCode", searchGroup.InvitedCode);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows && reader.Read())
                        {
                            groupId = reader.GetInt32(0);
                            resultGroup.GroupId = groupId;
                            resultGroup.GroupName = reader.GetString(1);
                            resultGroup.GroupIntro = reader.GetString(2);
                            string photoRoute = null;
                            if (!reader.IsDBNull(3))
                            {
                                photoRoute = Global.group_photo_url + reader.GetString(3);
                            }
                            resultGroup.GroupPhoto = photoRoute;
                            //確認身分
                            string strIsMember = "SELECT Accepted FROM [Group_User] WHERE [GroupId]=@groupId AND [UserId]=@userId";
                            using (SqlCommand cmdMatch = new SqlCommand(strIsMember, sqlConnection))
                            {
                                cmdMatch.Parameters.AddWithValue("@groupId", groupId);
                                cmdMatch.Parameters.AddWithValue("@userId", userId);
                                using (SqlDataReader readMatch = cmdMatch.ExecuteReader())
                                {
                                    if (readMatch.HasRows && readMatch.Read())
                                    {
                                        if (readMatch.GetInt32(0) == 0 || readMatch.GetInt32(0) == 3)
                                        {
                                            status = "alreadySend";
                                        }
                                        else
                                        {
                                            status = "isMember";
                                        }

                                    }
                                }
                            }
                            if (status == null)
                            {
                                status = "canApply";
                            }
                            resultGroup.Status = status;
                            return CreatedAtAction("GetGroupInfo", new { errorcode = -1, msg = "success get group", Group = resultGroup });
                        }
                        return CreatedAtAction("GetGroupInfo", new { errorcode = -402, msg = "can't find group" });
                    }
                }
                else //3.傳入的是名字
                {
                    Console.WriteLine("search group by name");
                    List<ReturnSearchGroup> resultGroup = new List<ReturnSearchGroup>();
                    string strGetGroup = @"DECLARE @Search NVARCHAR(20)
                                            SET @Search = @groupName
                                            SELECT GroupId,GroupName, GroupIntro,GroupPhoto
                                                FROM[GroupInfo]
                                                WHERE[GroupName] LIKE '%' + @Search + '%'";
                    using (SqlCommand cmd = new SqlCommand(strGetGroup, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@groupName", searchGroup.GroupName);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {

                                ReturnSearchGroup newObj = new ReturnSearchGroup();
                                groupId = reader.GetInt32(0);
                                newObj.GroupId = groupId;
                                newObj.GroupName = reader.GetString(1);
                                newObj.GroupIntro = reader.GetString(2); //確認身分
                                string photoRoute = null;
                                if (!reader.IsDBNull(3))
                                {
                                    photoRoute = Global.group_photo_url + reader.GetString(3);
                                }
                                newObj.GroupPhoto = photoRoute;

                                string strIsMember = "SELECT Accepted FROM [Group_User] WHERE [GroupId]=@groupId AND [UserId]=@userId";
                                using (SqlCommand cmdMatch = new SqlCommand(strIsMember, sqlConnection))
                                {
                                    cmdMatch.Parameters.AddWithValue("@groupId", groupId);
                                    cmdMatch.Parameters.AddWithValue("@userId", userId);
                                    using (SqlDataReader readerMember = cmdMatch.ExecuteReader())
                                    {
                                        if (readerMember.HasRows && readerMember.Read())
                                        {
                                            if (readerMember.GetInt32(0) == 0 || readerMember.GetInt32(0) == 3)
                                            {
                                                status = "alreadySend";
                                            }
                                            else
                                            {
                                                status = "isMember";
                                            }

                                        }
                                    }

                                }
                                if (status == null)
                                {
                                    status = "canApply";
                                }
                                newObj.Status = status;
                                resultGroup.Add(newObj);
                                status = null;
                            }
                            reader.Close();
                            return CreatedAtAction("GetGroupInfo", new { errorcode = -1, msg = "success get group", Group = resultGroup });
                        }
                        return CreatedAtAction("GetGroupInfo", new { errorcode = -402, msg = "can't find group" });
                    }

                }

            }

            //var groupInfo = await _context.GroupInfo.FindAsync(id);

            //if (groupInfo == null)
            //{
            //    return NotFound();
            //}

            //return groupInfo;
        }



    }
}
