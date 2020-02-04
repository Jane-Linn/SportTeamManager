using AppTest.CreateFakeData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateDataController : ControllerBase
    {

        [HttpGet]
        public ActionResult Create()
        {
            CreateUser createUser = new CreateUser();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strcreateUser = "INSERT INTO UserInfo(UserName, Email, Password) VALUES (@username,@email,@password)";
                using (SqlCommand cmd = new SqlCommand(strcreateUser, sqlConnection))
                {
                    for (int i = 0; i < CreateUser.users.Count; i++)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@username", CreateUser.users[i].Name);
                        cmd.Parameters.AddWithValue("@email", CreateUser.users[i].Email);
                        cmd.Parameters.AddWithValue("@password", CreateUser.users[i].Password);
                        cmd.ExecuteNonQuery();
                    }

                }
            }
            return CreatedAtAction("Create", new { errorcode = -1, msg = "success create"});
        }

    }
}
