using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using Microsoft.Data.SqlClient;
using System.Text;

namespace AppTest.Controllers.PersonalControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserContext _context;

        public LoginController(UserContext context)
        {
            _context = context;
        }

    

        // GET: api/Login
        [HttpGet("hello")]
        public string GetHello()
        {
            return "hello";
        }

      

        
        // POST: api/Login
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        //檢查是否有一樣的account和password 回傳token 或 error

            //先寫死了 之後藥房exception
        [HttpPost]
        public ActionResult PostUser(UserInfo user)
        {
            Console.WriteLine("login");
            //string account = user.Email;
            //string password = user.Password;
            
            using(SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {

                sqlConnection.Open();
                string sqlStrEmail = "SELECT [Password],[UserId] FROM [UserInfo] WHERE [Email]=@Email";
                using (SqlCommand cmdEmail = new SqlCommand(sqlStrEmail, sqlConnection))
                {
                    cmdEmail.Parameters.AddWithValue("@Email", user.Email);
                    //cmdEmail.Parameters.Add("@Password", System.Data.SqlDbType.VarChar).Value = user.Password;
                    using (SqlDataReader reader = cmdEmail.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {

                            if (reader.Read())
                            {
                                string findedPassword = reader[0].ToString();
                                int userId = reader.GetInt32(1);
                                reader.Close();
                                if (findedPassword.Equals(user.Password))
                                {
                                   
                                    string result = GenerateToken(user.Email.ToString());
                                    string sqlStrToken = "UPDATE [UserInfo] SET [Token]=(@param1) WHERE [Email]=@Email";
                                    using( SqlCommand cmdToken = new SqlCommand(sqlStrToken, sqlConnection))
                                    {
                                        cmdToken.Parameters.AddWithValue("@Email",user.Email);
                                        cmdToken.Parameters.AddWithValue("@param1", result);
                                        cmdToken.ExecuteNonQuery();
                                    }                                   
                                   
                                    
                                    cmdEmail.ExecuteNonQuery();
                                    sqlConnection.Close();
                                    return CreatedAtAction("PostUser", new { errorcode = -1, msg = "login success", Token = result,UserId=userId });
                                }
                                else
                                {
                                    reader.Close();
                                    cmdEmail.ExecuteNonQuery();
                                    sqlConnection.Close();
                                    return CreatedAtAction("PostUser", new { errorcode = 401, msg = "wrong password" });
                                }

                            }
                            //else
                            //{
                            //    reader.Close();
                            //    cmdEmail.ExecuteNonQuery();
                            //    sqlConnection.Close();
                            //    return CreatedAtAction("PostUser", new { errorcode = 402, msg = "can't find email" });
                            //}


                        }
                        reader.Close();
                        cmdEmail.ExecuteNonQuery();
                        sqlConnection.Close();
                    };
                };

            };
            return CreatedAtAction("PostUser", new { errorcode = 402, msg = "can't find email" });
            //_context.User.Add(user);
            //await _context.SaveChangesAsync();
            //return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);


        }

       
        public string GenerateToken(string email)
        {

            
            int ascii = Convert.ToInt32(email[0]);

            string token = email + "/" + ascii;
            return token;
        }
    }
}
