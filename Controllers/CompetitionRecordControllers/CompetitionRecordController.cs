using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;
using AppTest.InModels.GameRecordModel;
using Microsoft.Data.SqlClient;

namespace AppTest.Controllers.ComplicationRecordControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionRecordController : ControllerBase
    {
        private readonly UserContext _context;

        public CompetitionRecordController(UserContext context)
        {
            _context = context;
        }

        // POST: api/CompetitionRecord
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult> AddCompetitionRecord(CompetitionFromApp competitionFromApp)
        { 
            int competitionId = -1;
            Console.WriteLine("send a new competition record");
            Console.WriteLine(competitionFromApp.ToString());
            using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
               
                sqlConnection.Open();
                //1.新增賽事資訊
                string strAddGameRecord = @"INSERT INTO CompetitionInfo(GroupId, CompetitionName, TotalScore, CompetitionDate, CompetitionDescribe, Opponent, Status, CompetitionLocation)
                                                OUTPUT INSERTED.CompetitionId 
                                                VALUES(@groupId, @competitionName,@totalScore, convert(datetime,@recordDate, 20) , @describe, @opponent, @status, @location)
                                                 ";
                using (SqlCommand cmd = new SqlCommand(strAddGameRecord, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@groupId", competitionFromApp.Competition.GroupId);
                    cmd.Parameters.AddWithValue("@competitionName", competitionFromApp.Competition.CompetitionName);
                    cmd.Parameters.AddWithValue("@totalScore", competitionFromApp.Competition.TotalScore);
                    cmd.Parameters.AddWithValue("@recordDate", competitionFromApp.Competition.CompetitionDate);
                    //describe可以是null
                    cmd.Parameters.AddWithValue("@describe", competitionFromApp.Competition.CompetitionDescribe == null ? (object)DBNull.Value : competitionFromApp.Competition.CompetitionDescribe);
                    cmd.Parameters.AddWithValue("@opponent", competitionFromApp.Competition.Opponent);
                    
                    cmd.Parameters.AddWithValue("@status", competitionFromApp.Competition.Status);
                    //CompetitionLocation可以是null
                    cmd.Parameters.AddWithValue("@location", competitionFromApp.Competition.CompetitionLocation == null ? (object)DBNull.Value : competitionFromApp.Competition.CompetitionLocation);
                    //insert取得剛剛新增的資料的id
                    Console.WriteLine("insert成功");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            competitionId = reader.GetInt32(0);
                            Console.WriteLine("read成功");
                        }

                    }
                }
                //status 記在groupinfo裡要記錄輸贏次數
                string strUpdateWin = @"Update GroupInfo SET WinTime = WinTime+1 WHERE GroupId = @groupId";
                string strUpdateLose = @"Update GroupInfo SET LoseTime = LoseTime+1 WHERE GroupId = @groupId ";
                
                switch (competitionFromApp.Competition.Status)
                {
                    case 0:
                        using(SqlCommand cmd = new SqlCommand(strUpdateLose, sqlConnection))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@groupId", competitionFromApp.Competition.GroupId);
                            cmd.ExecuteNonQuery();
                        }
                        break;
                    case 1:
                        using (SqlCommand cmd = new SqlCommand(strUpdateWin, sqlConnection))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@groupId", competitionFromApp.Competition.GroupId);
                            cmd.ExecuteNonQuery();
                        }
                        break;
                }
            }
            //2. 新增各節紀錄 
            for (int i = 0; i < competitionFromApp.Competition.Quarters.Count(); i++)
            {
                Console.WriteLine("紀錄quarter");
                Competition_Quarter quarter = new Competition_Quarter();
                quarter.CompetitionId = competitionId;
                quarter.Quarter = competitionFromApp.Competition.Quarters.ElementAt(i).Quarter;
                quarter.QuarterScore = competitionFromApp.Competition.Quarters.ElementAt(i).QuarterScore;
                quarter.TF = competitionFromApp.Competition.Quarters.ElementAt(i).TF;
                quarter.OpponentScore = competitionFromApp.Competition.Quarters.ElementAt(i).OpponentScore;
                _context.Competition_Quarter.Add(quarter);
                await _context.SaveChangesAsync();
            }
            //3. 新增球員個人賽事記錄
            for (int i = 0; i < competitionFromApp.Competition.Players.Count(); i++)
            {
                Console.WriteLine("紀錄個人成績");
                Competition_User player = new Competition_User();
                player.Assists = competitionFromApp.Competition.Players.ElementAt(i).Assists;
                player.Blocks = competitionFromApp.Competition.Players.ElementAt(i).Blocks;
                player.DRB = competitionFromApp.Competition.Players.ElementAt(i).DRB;
                player.ORB = competitionFromApp.Competition.Players.ElementAt(i).ORB;
                player.PersonScore = competitionFromApp.Competition.Players.ElementAt(i).PersonScore;
                player.PF = competitionFromApp.Competition.Players.ElementAt(i).PF;
                player.Steals = competitionFromApp.Competition.Players.ElementAt(i).Steals;
                player.Turnovers = competitionFromApp.Competition.Players.ElementAt(i).Turnovers;
                player.UserId = competitionFromApp.Competition.Players.ElementAt(i).UserId;
                player.FGA = competitionFromApp.Competition.Players.ElementAt(i).FGA;
                player.FGM = competitionFromApp.Competition.Players.ElementAt(i).FGM;
                player.FGM3 = competitionFromApp.Competition.Players.ElementAt(i).FGM3;
                player.FG3 = competitionFromApp.Competition.Players.ElementAt(i).FG3;
                player.FTA = competitionFromApp.Competition.Players.ElementAt(i).FTA;
                player.FTM = competitionFromApp.Competition.Players.ElementAt(i).FTM;
                
                player.CompetitionId = competitionId;
                switch (competitionFromApp.Competition.Players.ElementAt(i).IsStart)
                {
                    case "true":
                        
                        player.IsStart = true;
                        break;
                    case "false":
                       
                        player.IsStart = false;
                        break;
                }
                _context.Competition_User.Add(player);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("AddCompetitionRecord", new { errorcode = -1, msg = "success add record" }); ;
        }


    }
}
