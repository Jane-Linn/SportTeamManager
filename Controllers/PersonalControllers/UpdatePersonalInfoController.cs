using AppTest.InModels.PersonalModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Controllers.PersonalControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdatePersonalInfoController : ControllerBase
    {
        [HttpPost]
        public ActionResult UpdateUserInfo(UpdatePersonalInfo personalInfo)
        {
            int userId = -1;
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                //1.先確認Token並把UserId抓下來
                //int userId = CheckToken(sqlConnection, groupInfo.Token);

                //1.先確認Token並把UserId抓下來
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", personalInfo.Token);
                    string[] account = personalInfo.Token.Split("/");
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
                                return CreatedAtAction("UpdateUserInfo", new { errorcode = 402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("UpdateUserInfo", new { errorcode = 402, msg = "can't find user" });
                }
                Console.WriteLine("UserId" + userId);

                string strUpdateInfo = "Update UserInfo SET UserName=@name,Gender=@gender, Birthday=@birthday WHERE UserId = @userId ";
                using (SqlCommand cmd = new SqlCommand(strUpdateInfo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("@name", personalInfo.UserName);
                    int gender = -1;
                    switch (personalInfo.Gender)
                    {
                        case "no gender info":
                            gender = 0;
                            break;
                        case "girl":
                            gender = 1;
                            break;
                        case "boy":
                            gender = 2;
                            break;
                        case "transgender":
                            gender = 3;
                            break;

                    }
                    cmd.Parameters.AddWithValue("@gender", gender);
                    if (!string.IsNullOrEmpty(personalInfo.Birthday))
                    {
                        cmd.Parameters.AddWithValue("@birthday", DateTime.Parse(personalInfo.Birthday));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@birthday", (object)DBNull.Value);
                    }
                  
                    cmd.ExecuteNonQuery();
                    return CreatedAtAction("UpdateUserInfo", new { errorcode = -1, msg = "success update personal info" });
                }
            }

        }
    }
}
