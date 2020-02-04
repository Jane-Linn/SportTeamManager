using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using Microsoft.Data.SqlClient;
using AppTest.InModels.PersonalModel;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AppTest.InModels;

namespace AppTest.Controllers.PersonalControllers
{
    [Route("api")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"}
        };
        private readonly string _folder;
        private readonly UserContext _context;
        public RegisterController(UserContext context)
        {
            // 把上傳目錄設為：wwwroot\UploadFolder
            _folder = Global.user_photo_route;
            _context = context;
            Console.WriteLine("%吃路徑 " + _folder);
        }



        // POST: api/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("Register")]
        public async Task<ActionResult> PostUser([FromForm]RegisterInput user)
        {
            int userId = -1;
            Console.WriteLine("register");
            
            if (user.Email == null || user.Password == null || user.UserName == null)
            {
                return CreatedAtAction("PostUser", new { errorcode = 401, msg = "data not complete" });
            }
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string sqlStrCheck = "SELECT [Email] FROM [UserInfo] WHERE [Email]=@Email";
                using (SqlCommand cmdCheck = new SqlCommand(sqlStrCheck, sqlConnection))
                {
                    cmdCheck.Parameters.AddWithValue("@Email", user.Email);
                    using (SqlDataReader reader = cmdCheck.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {

                                //已註冊過
                                reader.Close();
                                sqlConnection.Close();
                                return CreatedAtAction("PostUser", new { errorcode = 400, msg = "email already exist" });
                            }

                        }

                    }
                }
                //確認可以註冊喔!!!!!
                //1.insert其他的資料，取得UserId
                string sqlAddUser = @" INSERT INTO UserInfo(Email, Password, UserName,Gender)
                                                OUTPUT INSERTED.UserId 
                                                VALUES(@email, @password,@userName,@gender) ";
                using (SqlCommand cmd = new SqlCommand(sqlAddUser, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@userName", user.UserName);
                    cmd.Parameters.AddWithValue("@gender", 0);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            userId = reader.GetInt32(0);
                        }

                    }
                }

                //2.如果有photo放photo
                //(1)上傳照片
                if (user.UserPhoto != null)
                {
                    if (user.UserPhoto.Length > 0)
                    {
                        //儲存時用userId當作照片的名字
                        string[] types = user.UserPhoto.ContentType.Split("/");
                        string photoName = userId + "." + types[1];
                        var path = $"{_folder}{photoName}";//***********限制前端groupPhoto要加入groupName.JPG(檔案類型)
                        Console.WriteLine("Upload pic 照片存放地:" + path);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await user.UserPhoto.CopyToAsync(stream);

                        }
                        string strAddPhotoName = @"Update UserInfo SET UserPhoto=@userPhotoName WHERE UserId = @userId";
                        using(SqlCommand cmd = new SqlCommand(strAddPhotoName, sqlConnection))
                        {
                            cmd.Parameters.AddWithValue("@userPhotoName", photoName);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

            }
            return CreatedAtAction("PostUser", new { errorcode = -1, msg = "register success" });
        }

        //get個人圖片
        [HttpGet("person_photo/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            Console.WriteLine("Download pic");
            if (string.IsNullOrEmpty(fileName))
            {
                return CreatedAtAction("Download", new { errorcode = 401, msg = "no found photo" });
            }

            var path = $"{_folder}{fileName}";
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            // 回傳檔案到 Client 需要附上 Content Type，否則瀏覽器會解析失敗。
            return new FileStreamResult(memoryStream, _contentTypes[Path.GetExtension(path).ToLowerInvariant()]);
        }
        //POST: api/update_user_photo
        //新增更新單一使用者照片
        [HttpPost("update_user_photo")]
        public async Task<IActionResult> UpdatePhoto([FromForm] ImageModel file)
        {
            int userId = -1;
            //(1)上傳照片
            Console.WriteLine("Update user photo");
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCheckToken = "SELECT [UserId] FROM [UserInfo] WHERE [Token]=@Token AND [Email]=@ACCOUNT";
                using (SqlCommand cmdReadUserId = new SqlCommand(strCheckToken, sqlConnection))
                {
                    cmdReadUserId.Parameters.AddWithValue("@Token", file.Token);
                    string[] account = file.Token.Split("/");
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
                                return CreatedAtAction("UpdatePhoto", new { errorcode = -402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("UpdatePhoto", new { errorcode = -402, msg = "can't find user" });
                }

                Console.WriteLine("UserId" + userId);

                string strAddPhotoName = @"Update UserInfo SET UserPhoto=@userPhotoName WHERE UserId = @userId";
                using (SqlCommand cmdAddId = new SqlCommand(strAddPhotoName, sqlConnection))
                {
                    if (file.Photo != null)
                    {

                        //上傳照片
                        if (file.Photo.Length > 0)
                        {
                            //儲存時用userId當作照片的名字
                            string[] types = file.Photo.ContentType.Split("/");
                            string photoName = userId + "." + types[1];
                            var path = $"{_folder}{photoName}";//***********限制前端groupPhoto要加入groupName.JPG(檔案類型)
                            Console.WriteLine("Upload pic 照片存放地:" + path);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.Photo.CopyToAsync(stream);

                            }
                            
                            using (SqlCommand cmd = new SqlCommand(strAddPhotoName, sqlConnection))
                            {
                                cmd.Parameters.AddWithValue("@userPhotoName", photoName);
                                cmd.Parameters.AddWithValue("@userId", userId);
                                cmd.ExecuteNonQuery();
                                return CreatedAtAction("UpdatePhoto", new { errorcode = -1, msg = "success update user photo" });
                            }
                        }
                    }
                    return CreatedAtAction("UpdatePhoto", new { errorcode = 400, msg = "no photo" });
                }
            }
        }

    }
}
