using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTest.InModels;
using AppTest.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AppTest.Controllers
{
    //**********很醜要改
    //
    [Route("api/Group")]
    public class BuildGroupImageController : ControllerBase
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
        public BuildGroupImageController(IWebHostEnvironment env, UserContext context)
        {
            // 把上傳目錄設為：wwwroot\UploadFolder
            _folder = Global.group_photo_route;
            _context = context;
            Console.WriteLine("%吃路徑 " + _folder);
        }








        // POST: api/Group/new_build_image
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("new_build_image")]
        public async Task<ActionResult> PostGroupInfo([FromForm]AddGroupFromAppPhoto file)
        {
            Console.WriteLine("build a group" + file.Token);
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
                                return CreatedAtAction("PostGroupInfo", new { errorcode = -402, msg = "can't find user" });
                            }
                        }
                    }
                }
                if (userId == -1)
                {
                    return CreatedAtAction("PostGroupInfo", new { errorcode = -402, msg = "can't find user" });
                }

                Console.WriteLine("UserId" + userId);
                //2.確認這個group有沒有人創了
                string strCheckName = "SELECT [GroupName] FROM [GroupInfo] WHERE [GroupName]=@GroupName";
                using (SqlCommand cmdCheckName = new SqlCommand(strCheckName, sqlConnection))
                {
                    cmdCheckName.Parameters.AddWithValue("@GroupName", file.GroupName);
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
                string addIntoSql = @"INSERT INTO [GroupInfo]([GroupName],[Age],[WinTime],[LoseTime],[InvitedCode],[GroupIntro]) 
                                        VALUES(@name, @age, @win, @lose, @invite,@groupIntro) 
                                        (SELECT [GroupId] FROM [GroupInfo] WHERE [GroupName]=@GroupName) ";

                using (SqlCommand cmdAddInfo = new SqlCommand(addIntoSql, sqlConnection))
                {
                    invitedCode = makeInvitedCode(file.GroupName);
                    cmdAddInfo.Parameters.AddWithValue("@name", file.GroupName);
                    cmdAddInfo.Parameters.AddWithValue("@age", GetGroupAge(file.Age));
                    cmdAddInfo.Parameters.AddWithValue("@win", 0);
                    cmdAddInfo.Parameters.AddWithValue("@lose", 0);
                    cmdAddInfo.Parameters.AddWithValue("@invite", invitedCode);
                    if (file.GroupIntro == null)
                    {
                        file.GroupIntro = "no groupIntro";
                    }

                    cmdAddInfo.Parameters.AddWithValue("@groupIntro", file.GroupIntro);
                    cmdAddInfo.Parameters.AddWithValue("@GroupName", file.GroupName);
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
                //4.把你和groupId放進Group_User關聯中繼表中
                string addId = " INSERT INTO [Group_User]([GroupId],[UserId],[IsManager],[Accepted],JoinGroupDate) VALUES(@group, @user, @manager,@accepted, @joinDate)";
                string addRoute = "UPDATE GroupInfo SET GroupPhoto = @fileName WHERE GroupId = @id";
                using (SqlCommand cmdAddId = new SqlCommand(addId, sqlConnection))
                {
                    if (file.GroupPhoto != null)
                    {

                        //上傳照片
                        if (file.GroupPhoto.Length > 0)
                        {
                            var path = $"{_folder}{file.GroupPhoto.FileName}";//***********限制前端groupPhoto要加入groupName.JPG(檔案類型)
                            Console.WriteLine("Upload pic 照片存放地:" + path);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.GroupPhoto.CopyToAsync(stream);
                            }
                            //把照片路徑存到資料表(不是存path是存他們可以請求照片的路徑，這裡直接存圖片名稱，要給路徑的時候在套進路徑)
                            using (SqlCommand cmdAddPhoto = new SqlCommand(addRoute, sqlConnection))
                            {
                                cmdAddPhoto.Parameters.AddWithValue("@fileName", file.GroupPhoto.FileName);
                                cmdAddPhoto.Parameters.AddWithValue("@id", groupId);
                                cmdAddPhoto.ExecuteNonQuery();
                            }
                        }
                    }

                    cmdAddId.Parameters.AddWithValue("@group", groupId);
                    cmdAddId.Parameters.AddWithValue("@user", userId);
                    cmdAddId.Parameters.AddWithValue("@manager", 1);
                    cmdAddId.Parameters.AddWithValue("@accepted", 1);
                    cmdAddId.Parameters.AddWithValue("@joinDate", DateTime.Now);
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

        // POST: api/Group/update_group_photo
        //單純上傳更換團體照片
        [HttpPost("update_group_photo")]
        public IActionResult UploadPhoto([FromForm]ImageModel file)
        {
            Console.WriteLine("Update group photo");
            //上傳照片
            if (file.Photo != null)
            {
                if (file.Photo.Length > 0)
                {
                    var path = $"{_folder}{file.Photo.FileName}";//***********限制前端groupPhoto要加入groupName.JPG(檔案類型)
                    Console.WriteLine("Upload pic 照片存放地:" + path);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.Photo.CopyToAsync(stream);
                    }

                    using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
                    {
                        sqlConnection.Open();
                        string addPhoto = "UPDATE GroupInfo SET GroupPhoto = @fileName WHERE GroupId = @id";

                        //判斷沒有找到對應的group回傳找不到此球隊
                        //把照片路徑存到資料表(不是存path是存他們可以請求照片的路徑，這裡直接存圖片名稱，要給路徑的時候在套進路徑)
                        using (SqlCommand cmdAddPhoto = new SqlCommand(addPhoto, sqlConnection))
                        {
                            cmdAddPhoto.Parameters.AddWithValue("@fileName", file.Photo.FileName);
                            cmdAddPhoto.Parameters.AddWithValue("@id", file.GroupId);
                            if (cmdAddPhoto.ExecuteNonQuery() == 0)
                            {
                                return CreatedAtAction(nameof(UploadPhoto), new { errorcode = 401, msg = "can't find group" });
                            }
                            return CreatedAtAction("UploadPhoto", new { errorcode = -1, msg = "success update group photo" });
                        }
                    }
                }
            }
            return CreatedAtAction("UploadPhoto", new { errorcode = 400, msg = "no photo" });

        }




        //get group photo
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            Console.WriteLine("Download group pic");
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


    }
}


