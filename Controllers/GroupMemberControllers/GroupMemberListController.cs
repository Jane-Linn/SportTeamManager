using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using AppTest.InModels.MemberListModel;
using Microsoft.Data.SqlClient;
using AppTest.InModels;

//1.獲得團隊成員名單 (排序:管理員在前 成員依照參加的比賽時間排，在按照成績排)
//2.刪除人/指定/取消管理員(同一支:更改成員身分)
//3.自己退出球隊
//4.尋找成員主動加入(user email要多加一個可以按接受的cell)
//5.給背號


namespace AppTest.Controllers.GroupMemberControllers
{
    [Route("api/Member")]
    [ApiController]
    public class GroupMemberListController : ControllerBase
    {
        private readonly UserContext _context;
        private List<ShowMember> memberList;

        public GroupMemberListController(UserContext context)
        {
            _context = context;
            memberList = new List<ShowMember>();
        }

        // GET: api/Member/5
        // 取得名單
        [HttpGet("{groupId}")]
        public async Task<ActionResult<IEnumerable<ShowMember>>> GetMemberList([FromRoute] int groupId)
        {
            Console.WriteLine("show all member in this group " + groupId);

            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCreate = @"SELECT Group_User.UserId, UserName,UniformNumber,IsManager,LastGameDate,UserPhoto
                                      FROM [Group_User] ,[UserInfo]
                                      WHERE Group_User.GroupId = @groupId
                                        AND UserInfo.UserId = Group_User.UserId 
	                                    AND Group_User.UserId IN 
	                                      (SELECT UserId 
	                                         FROM Group_User 
		                                     WHERE GroupId= @groupId
	                                    AND Accepted IN (1,2))
                                      ORDER BY isManager DESC, JoinGroupDate DESC";
                await using (SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    await using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShowMember newObj = new ShowMember();
                                newObj.UserId = reader.GetInt32(0);
                                newObj.UserName = reader.GetString(1);

                                newObj.UniformNumber = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                switch (reader.GetInt32(3))
                                {
                                    case 0:
                                        newObj.Role = "is member";
                                        break;
                                    case 1:
                                        newObj.Role = "is manager";
                                        break;
                                }
                                newObj.LastGameDate = reader.IsDBNull(4) ? "haven't play yet" : reader.GetDateTime(4).ToString("yyyy-MM-dd");
                                string photoRoute = null;
                                if (!reader.IsDBNull(5))
                                {
                                    photoRoute = Global.user_photo_url + reader.GetString(5);
                                }

                                newObj.UserPhoto = photoRoute;
                                memberList.Add(newObj);
                            }
                            return CreatedAtAction("GetMemberList", new { errorcode = -1, msg = "success get memberList", MemberList = memberList });
                        }
                        else
                        {

                            return CreatedAtAction("GetMemberList", new { errorcode = 400, msg = "no member in this group" });
                        }
                    }

                }
            }
        }


        //管理員更改別人的身分
        // POST: api/Member/5/change_3
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("{groupId}/change_{targetUserId}")]
        public async Task<ActionResult<Group_User>> ChangeMemberRole([FromRoute]int groupId, int targetUserId, [FromBody] ChangeRole changeRole)
        {
            //token是操作的人 (必須是groupmanager或是要被刪除的人本人)
            Console.WriteLine("change user role" + targetUserId);
            int operateUserId = -1;
            int operateRole = 0;
            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string + " ;MultipleActiveResultSets = true"))
            {
                sqlConnection.Open();
                string strCheckToken = @"SELECT Group_User.UserId,IsManager
                                          FROM [UserInfo],[Group_User] 
                                          WHERE Group_User.GroupId = @groupId 
                                          AND Group_User.UserId = UserInfo.UserId 
                                          AND USerInfo.UserId IN
                                            (SELECT UserId
                                             From UserInfo 
                                             WHERE [Token]=@Token AND [Email]=@ACCOUNT)";
                await using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", changeRole.Token);
                    string[] account = changeRole.Token.Split("/");
                    cmdReadUserId.Parameters.AddWithValue("@ACCOUNT", account[0]);
                    cmdReadUserId.Parameters.AddWithValue("@groupId", groupId);
                    await using (SqlDataReader readUserId = cmdReadUserId.ExecuteReader())
                    {
                        if (readUserId.HasRows)
                        {
                            if (readUserId.Read())
                            {
                                operateUserId = (int)readUserId[0];
                                operateRole = readUserId.GetInt32(1);
                            }

                        }
                    }
                }
                if (operateUserId == -1)
                {
                    return CreatedAtAction("ChangeMemberRole", new { errorcode = 401, msg = "can't find operator" });
                }
                //是管理員才能操作
                if (operateRole == 1)
                {
                    switch (changeRole.ChangeEvent)
                    {
                        case "change to manager":
                            //target變成管理員
                            string strBecomeManager = @"UPDATE Group_User SET IsManager =1 WHERE UserId =@targetUser AND GroupId = @groupId AND (Accepted IN (1,2))";
                            await using (SqlCommand cmd = new SqlCommand(strBecomeManager, sqlConnection))
                            {
                                cmd.Parameters.AddWithValue("@targetUser", targetUserId);
                                cmd.Parameters.AddWithValue("@groupId", groupId);
                                if (cmd.ExecuteNonQuery() == 0)
                                {
                                    return CreatedAtAction(nameof(ChangeMemberRole), new { errorcode = 403, msg = "can't find user" });
                                }
                                return CreatedAtAction(nameof(ChangeMemberRole), new { errorcode = -1, msg = "success change to manager" });
                            }
                        case "change to normal player":
                            //target變成一般成員
                            string strBecomeNormal = @"UPDATE Group_User SET IsManager =0 WHERE UserId =@targetUser AND GroupId = @groupId AND (Accepted IN (1,2))";
                            await using (SqlCommand cmd = new SqlCommand(strBecomeNormal, sqlConnection))
                            {
                                cmd.Parameters.AddWithValue("@targetUser", targetUserId);
                                cmd.Parameters.AddWithValue("@groupId", groupId);
                                if (cmd.ExecuteNonQuery() == 0)
                                {
                                    return CreatedAtAction(nameof(ChangeMemberRole), new { errorcode = 403, msg = "can't find user" });
                                }
                                return CreatedAtAction(nameof(ChangeMemberRole), new { errorcode = -1, msg = "success change to normal" });
                            }
                    }
                }
                else
                {
                    return CreatedAtAction("ChangeMemberRole", new { errorcode = 400, msg = "you are not manager" });
                }
            }
            return CreatedAtAction("ChangeMemberRole", new { errorcode = 400, msg = "you are not manager" });
        }

        // DELETE: api/Member/5/delete_3
        // 退出團隊(解除某user和某group的關係)
        [HttpPost("{groupId}/delete_{targetUserId}")]
        public async Task<ActionResult<Group_User>> DeleteGroup_User([FromRoute] int groupId, int targetUserId, [FromBody] SendToken token)
        {
            //token是操作的人 (必須是groupmanager或是要被刪除的人本人)
            Console.WriteLine("delete user" + targetUserId + "from group" + groupId);
            int operateUserId = -1;
            int operateRole = 0;
            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                Console.WriteLine("1");
                string strCheckToken = @"SELECT Group_User.UserId,IsManager 
                                          FROM [UserInfo],[Group_User] 
                                          WHERE Group_User.GroupId = @groupId 
                                          AND Group_User.UserId = UserInfo.UserId 
                                          AND USerInfo.UserId IN
                                            (SELECT UserId
                                             From UserInfo 
                                             WHERE [Token]=@Token AND [Email]=@ACCOUNT)";
                await using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    Console.WriteLine("1");
                    cmdReadUserId.Parameters.AddWithValue("@Token", token.Token);
                    string[] account = token.Token.Split("/");
                    cmdReadUserId.Parameters.AddWithValue("@ACCOUNT", account[0]);
                    cmdReadUserId.Parameters.AddWithValue("@groupId", groupId);
                    await using (SqlDataReader readUserId = cmdReadUserId.ExecuteReader())
                    {
                        Console.WriteLine("1");
                        if (readUserId.HasRows)
                        {
                            if (readUserId.Read())
                            {
                                operateUserId = (int)readUserId[0];
                                operateRole = readUserId.GetInt32(1);
                            }

                        }
                    }
                }
                if (operateUserId == -1)
                {
                    return CreatedAtAction("DeleteGroup_User", new { errorcode = 401, msg = "can't find operator" });
                }
                //成功得到操作者id
                //確認他可以操作(如果可以就刪除)
                //1.確認是不是本人(要確認是不是最後一個管理員)
                Console.WriteLine("1");
                if (operateUserId == targetUserId)
                {
                    if (operateRole == 1)
                    {
                        //是管理員要離開group
                        string strCheckManagerCount = @"SELECT COUNT(UserId) 
                                                            FROM Group_User 
                                                            WHERE IsManager =1 AND GroupId = @groupId";
                        await using (SqlCommand cmd = new SqlCommand(strCheckManagerCount, sqlConnection))
                        {

                            cmd.Parameters.AddWithValue("@groupId", groupId);
                            if ((Int32)cmd.ExecuteScalar() == 1)
                            {
                                //就剩你一個了離不開的
                                return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = 400, msg = "can't delete, you are the only manager in group" });
                            }
                        }
                    }
                    //不是管理員或是還有其他管理員，就離開吧
                    string deleteUser = @"DELETE FROM Group_User WHERE GroupId = @groupId AND UserId = @removeUser AND (Accepted IN (1,2))";
                    await using (SqlCommand cmd = new SqlCommand(deleteUser, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@groupId", groupId);
                        cmd.Parameters.AddWithValue("@removeUser", targetUserId);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = 403, msg = "can't find user" });
                        }
                    }
                    return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = -1, msg = "success delete" });

                }//2.刪別人(確認操作者是不是管理員)
                else
                {
                    if (operateRole == 1)
                    {//是管理員可以刪別人
                        string deleteUser = @"DELETE FROM Group_User WHERE GroupId = @groupId AND UserId = @removeUser AND (Accepted IN (1,2))";
                        await using (SqlCommand cmd = new SqlCommand(deleteUser, sqlConnection))
                        {
                            cmd.Parameters.AddWithValue("@groupId", groupId);
                            cmd.Parameters.AddWithValue("@removeUser", targetUserId);
                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = 403, msg = "can't find user" });
                            }
                        }
                        return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = -1, msg = "success delete" });

                    }
                    else
                    {
                        //你不是管理員不能刪別人
                        return CreatedAtAction(nameof(DeleteGroup_User), new { errorcode = 401, msg = "you are not manager can't remove other" });
                    }
                }
            }
        }

        //更改別人的背號
        //POST: api/Member/5/give_number_3
        [HttpPost("{groupId}/give_number_{targetUserId}")]
        public async Task<ActionResult<Group_User>> PutUniformNumber([FromRoute]int groupId, int targetUserId, [FromBody] PutUniform putUniform)
        {
            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strPutNumber = "UPDATE Group_User SET UniformNumber = @uniformNumber WHERE GroupId = @groupId AND UserId = @targetId AND (Accepted IN (1,2))";
                await using (SqlCommand cmd = new SqlCommand(strPutNumber, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@uniformNumber", putUniform.UniFormNumber);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@targetId", targetUserId);
                    cmd.ExecuteNonQuery();
                    return CreatedAtAction(nameof(PutUniformNumber), new { errorcode = -1, msg = "success put uniformNumber" });
                }
            }
        }


    }
}
