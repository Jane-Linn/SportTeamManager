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
    public class GetInvitedController : ControllerBase
    {
        private readonly UserContext _context;

        public GetInvitedController(UserContext context)
        {
            _context = context;
        }

        //POST: api/Group/invited_code
        [HttpPost("invited_code")]
        public ActionResult GetGroupInfo(GetGroupFromId groupInfo)
        {
            Console.WriteLine("get group invited code");
            string invitedCode = null;
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strGetInviteCode = "SELECT [InvitedCode] FROM [GroupInfo] WHERE [GroupId]=@groupId";
                using (SqlCommand cmdtakeCode = new SqlCommand(strGetInviteCode, sqlConnection))
                {
                    cmdtakeCode.Parameters.AddWithValue("@groupId", groupInfo.GroupId);
                    using (SqlDataReader readCode = cmdtakeCode.ExecuteReader())
                    {
                        if (readCode.HasRows)
                        {
                            if (readCode.Read())
                            {
                                invitedCode = readCode[0].ToString();
                                return CreatedAtAction("GetGroupInfo", new { errorcode = -1, msg = "success get invitedCode", InvtedCode = invitedCode });
                            }
                            else
                            {
                                return CreatedAtAction("GetGroupInfo", new { errorcode = -402, msg = "can't find invitedCode" });
                            }
                        }
                    }
                }
            }


            return CreatedAtAction("GetGroupInfo", new { errorcode = -402, msg = "can't find invitedCode" });
        }
    }
}
