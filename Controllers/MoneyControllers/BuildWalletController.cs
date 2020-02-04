using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTest.InModels;
using AppTest.InModels.WalletModel;
using AppTest.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AppTest.Controllers.MoneyControllers
{
    //*******未指定特定成員才能更改
    //***************一定有方法可以呼叫其他已經寫好的get method對吧!!!!?????
    [Route("api/Group/wallet")]
    public class BuildWalletController : ControllerBase
    {
        private readonly UserContext _context;
        public BuildWalletController(UserContext context)
        {
            _context = context;
        }
        //GET: api/Group/4
        //回傳紀錄
        //沒有帳本:回傳沒有 有的話: 依照日期降序列出所有紀錄
        [HttpGet("{groupId}")]
        public ActionResult ShowGroupWallet([FromRoute]int groupId)
        {
            Console.WriteLine("show all record in this group "+groupId);
            decimal totalMoney = 0;
            List<ShowWalletRecord> records = new List<ShowWalletRecord>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCreate = "SELECT WalletRecordId, MoneyTrack,WalletDescribe,RecordDate FROM [WalletInfo] WHERE GroupId = @groupId ORDER BY RecordDate ASC  ";
                using (SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShowWalletRecord newObj = new ShowWalletRecord();
                                newObj.WalletRecordId = reader.GetInt32(0);
                                newObj.MoneyTrack = reader.GetDecimal(1);
                                newObj.WalletDescribe = reader.GetString(2);
                                newObj.RecordDate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
                                totalMoney += newObj.MoneyTrack;
                                newObj.TotalMoney = totalMoney;
                                records.Add(newObj);
                            }
                            return CreatedAtAction("ShowGroupWallet", new { errorcode = -1, msg = "success get wallet record", WalletRecords = records });
                        }
                        else
                        {

                            return CreatedAtAction("ShowGroupWallet", new { errorcode = 400, msg = "the wallet yet found" });
                        }
                    }

                }
            }
        }
        ////***********未判斷沒有groupId的狀況
        ////建立wallet
        //[HttpPatch("new_build/{groupId}")]
        //public ActionResult CreateAWallet([FromRoute] int groupId)
        //{
        //    using( SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
        //    {
        //        sqlConnection.Open();
        //        string strCreate = "UPDATE GroupInfo SET WalletId = GroupId WHERE GroupId =@groupId";
        //        using(SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
        //        {
        //            cmd.Parameters.AddWithValue("@groupId", groupId);
        //            cmd.ExecuteNonQuery();
        //            return CreatedAtAction("CreateAWallet", new { errorcode = -1, msg = "success create wallet" });
        //        }
        //    }

        //}

        //記帳(新增紀錄)
        [HttpPost("{groupId}")]
        public ActionResult AddRecordInWallet([FromBody] AddMoneyRecord moneyRecord, [FromRoute] int groupId)
        {
            Console.WriteLine("add new money record");
            decimal totalMoney = 0;
            List<ShowWalletRecord> records = new List<ShowWalletRecord>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCreate = @"INSERT INTO WalletInfo(MoneyTrack,WalletDescribe,RecordDate,GroupId) 
                                        VALUES(@moneyTract, @WalletDescribe, convert(datetime,@recordDate, 20), @groupId)
                                        SELECT WalletRecordId, MoneyTrack, WalletDescribe, RecordDate FROM[WalletInfo] WHERE GroupId = @groupId ORDER BY RecordDate ASC
                                        ";
                using (SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@moneyTract", moneyRecord.MoneyTrack);
                    cmd.Parameters.AddWithValue("@WalletDescribe", moneyRecord.WalletDescribe);
                    cmd.Parameters.AddWithValue("@recordDate", moneyRecord.RecordDate);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.HasRows && reader.Read())
                        {
                            ShowWalletRecord newObj = new ShowWalletRecord();
                            newObj.WalletRecordId = reader.GetInt32(0);
                            newObj.MoneyTrack = reader.GetDecimal(1);
                            newObj.WalletDescribe = reader.GetString(2);
                            newObj.RecordDate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
                            totalMoney += newObj.MoneyTrack;
                            newObj.TotalMoney = totalMoney;
                            records.Add(newObj);
                        }
                    }
                    //return ShowGroupWallet(groupId);
                    return CreatedAtAction(nameof(AddRecordInWallet), new { errorcode = -1, msg = "success add record", WalletRecords = records });
                }
            }
        }

        //Delete:api/Group/wallet/{groupId}/{wallet_record_id}
        [HttpDelete("{groupId}/{wallet_record_id}")]
        public ActionResult DeleteRecordInWallet([FromRoute] int groupId, [FromRoute]int wallet_record_id)
        {
            Console.WriteLine("delete a money record");
            decimal totalMoney = 0;
            List<ShowWalletRecord> records = new List<ShowWalletRecord>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCreate = @"DELETE FROM WalletInfo WHERE WalletRecordId = @recordId
                                       SELECT WalletRecordId, MoneyTrack, WalletDescribe, RecordDate FROM[WalletInfo] WHERE GroupId = @groupId ORDER BY RecordDate ASC";
                using (SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@recordId", wallet_record_id);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShowWalletRecord newObj = new ShowWalletRecord();
                                newObj.WalletRecordId = reader.GetInt32(0);
                                newObj.MoneyTrack = reader.GetDecimal(1);
                                newObj.WalletDescribe = reader.GetString(2);
                                newObj.RecordDate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
                                totalMoney += newObj.MoneyTrack;
                                newObj.TotalMoney = totalMoney;
                                records.Add(newObj);
                            }
                            return CreatedAtAction("DeleteRecordInWallet", new { errorcode = -1, msg = "success get wallet record", WalletRecords = records });
                        }
                        else
                        {

                            return CreatedAtAction("DeleteRecordInWallet", new { errorcode = 400, msg = "the wallet yet found" });
                        }
                    }
                  
                }
            }
        }

        //修改
        //Post:api/Group/wallet/{groupId}/{wallet_record_id}
        [HttpPost("{groupId}/{wallet_record_id}")]
        public ActionResult UpdateRecordInWallet([FromRoute] int groupId, [FromRoute]int wallet_record_id,[FromBody] AddMoneyRecord moneyRecord)
        {
            Console.WriteLine("change a money record");
            decimal totalMoney = 0;
            List<ShowWalletRecord> records = new List<ShowWalletRecord>();
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strCreate = @"Update WalletInfo SET MoneyTrack=@moneyTract , WalletDescribe=@WalletDescribe,RecordDate=@recordDate WHERE WalletRecordId = @recordId
                                       SELECT WalletRecordId, MoneyTrack, WalletDescribe, RecordDate FROM[WalletInfo] WHERE GroupId = @groupId ORDER BY RecordDate ASC";
                using (SqlCommand cmd = new SqlCommand(strCreate, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@moneyTract", moneyRecord.MoneyTrack);
                    cmd.Parameters.AddWithValue("@WalletDescribe", moneyRecord.WalletDescribe);
                    cmd.Parameters.AddWithValue("@recordDate", moneyRecord.RecordDate);
                    cmd.Parameters.AddWithValue("@recordId", wallet_record_id);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ShowWalletRecord newObj = new ShowWalletRecord();
                                newObj.WalletRecordId = reader.GetInt32(0);
                                newObj.MoneyTrack = reader.GetDecimal(1);
                                newObj.WalletDescribe = reader.GetString(2);
                                newObj.RecordDate = reader.GetDateTime(3).ToString("yyyy-MM-dd");
                                totalMoney += newObj.MoneyTrack;
                                newObj.TotalMoney = totalMoney;
                                records.Add(newObj);
                            }
                            return CreatedAtAction("UpdateRecordInWallet", new { errorcode = -1, msg = "success get wallet record", WalletRecords = records });
                        }
                        else
                        {

                            return CreatedAtAction("UpdateRecordInWallet", new { errorcode = 400, msg = "the wallet yet found" });
                        }
                    }
                    
                }
            }
        }


    }
}
