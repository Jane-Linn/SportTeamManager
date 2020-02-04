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
using AppTest.InModels.PersonalModel;

namespace AppTest.Controllers.PersonalControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowPersonalController : ControllerBase
    {
        private readonly UserContext _context;

        public ShowPersonalController(UserContext context)
        {
            _context = context;
        }

        // Post: api/ShowPersonal
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ShowPersonalInfo>>> GetUserInfo(SendToken token)
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
                            else
                            {
                                return CreatedAtAction("GetUserInfo", new { errorcode = 402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("GetUserInfo", new { errorcode = 402, msg = "can't find user" });
                }
                Console.WriteLine("UserId" + userId);

                string strGetInfo = @"SELECT  UserName, Email, UserPhoto, Gender, Birthday,totalScore,ROUND(CAST(madeTotal AS float)/CAST(attendTotal AS float),2) as FieldGoal FROM
	                                    (SELECT  UserId,SUM(PersonScore) as totalScore,sum(FGA+FG3+FTA) as attendTotal,sum(FGM+FGM3+FTM) as madeTotal
		                                    FROM Competition_User
		                                    WHERE UserId =@userId
		                                    Group by UserId
		                                    )as tmp
                                        JOIN
	                                    (SELECT UserId,UserName, Email, UserPhoto, Gender, Birthday
		                                    FROM  UserInfo 
		                                    WHERE UserId = @userId) as tmp2
                                        ON tmp.UserId=tmp2.UserId ";
                using(SqlCommand cmd = new SqlCommand(strGetInfo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.HasRows && reader.Read())
                        {
                            ShowPersonalInfo personalInfo = new ShowPersonalInfo();
                            personalInfo.UserName = reader.GetString(0);
                            personalInfo.Email = reader.GetString(1);
                            personalInfo.UserPhoto = reader.IsDBNull(2) ? "no user photo": (Global.user_photo_url+reader.GetString(2));
                            switch(reader.GetInt32(3)){
                                case 0:
                                    personalInfo.Gender = "no gender info";
                                    break;
                                case 1:
                                    personalInfo.Gender = "girl";
                                    break;
                                case 2:
                                    personalInfo.Gender = "boy";
                                    break;
                                case 3:
                                    personalInfo.Gender = "transgender";
                                    break;
                            }
                            personalInfo.CareerPoint = reader.GetInt32(5);
                            personalInfo.FieldGoal = reader.GetDouble(6);
                            return CreatedAtAction("GetUserInfo", new { errorcode = -1, msg = "success get personal info", UserInfo= personalInfo });
                        }
                    }
                }
                return CreatedAtAction("GetUserInfo", new { errorcode = 403, msg = "can't get info"});
            }
        }


        //// GET: api/ShowPersonal/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<UserInfo>> GetUserInfo(int? id)
        //{
        //    var userInfo = await _context.UserInfo.FindAsync(id);

        //    if (userInfo == null)
        //    {
        //        return NotFound();
        //    }

        //    return userInfo;
        //}

        //// PUT: api/ShowPersonal/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://aka.ms/RazorPagesCRUD.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUserInfo(int? id, UserInfo userInfo)
        //{
        //    if (id != userInfo.UserId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(userInfo).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserInfoExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/ShowPersonal
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://aka.ms/RazorPagesCRUD.
        //[HttpPost]
        //public async Task<ActionResult<UserInfo>> PostUserInfo(UserInfo userInfo)
        //{
        //    _context.UserInfo.Add(userInfo);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUserInfo", new { id = userInfo.UserId }, userInfo);
        //}

        //// DELETE: api/ShowPersonal/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult<UserInfo>> DeleteUserInfo(int? id)
        //{
        //    var userInfo = await _context.UserInfo.FindAsync(id);
        //    if (userInfo == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.UserInfo.Remove(userInfo);
        //    await _context.SaveChangesAsync();

        //    return userInfo;
        //}

        //private bool UserInfoExists(int? id)
        //{
        //    return _context.UserInfo.Any(e => e.UserId == id);
        //}
    }
}
