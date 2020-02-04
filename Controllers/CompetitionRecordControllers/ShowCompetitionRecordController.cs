using AppTest.InModels.GameRecordModel;
using AppTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Controllers.CompetitionRecordControllers
{
    /// <summary>
    ///寫很醜!!!!!!!!!!!!!!!!!!!應該有更好的寫法
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ShowCompetitionRecordController : ControllerBase

    {
        //在比賽紀錄的頁面:
        //1. show 這個group全部的比賽概要
        //2. show 單個比賽的詳細資料
        //3. show 單次比賽個人的詳細資料

        private readonly UserContext _context;
        public ShowCompetitionRecordController(UserContext context)
        {
            _context = context;
        }

        //GET: api/ShowCompetitionRecord/5
        // 得全部的比賽紀錄
        [HttpGet("{groupId}")]
        public async Task<ActionResult> GetAllCompetition([FromRoute]int groupId)
        {
            Stopwatch stopWacth = new Stopwatch();
            stopWacth.Start();
            Console.WriteLine("getallCompetition" + groupId);
            List<CompetitionIntro> competitionIntros = new List<CompetitionIntro>();
            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                string strGetInfo = @"SELECT tmp.CompetitionId,CompetitionDate,CompetitionLocation,CompetitionName,totalScore,opponentTotal,Opponent FROM
	                                    (SELECT  Competition_Quarter.CompetitionId,SUM([QuarterScore]) as totalScore,sum([OpponentScore]) as opponentTotal
		                                    FROM Competition_Quarter
		                                    WHERE Competition_Quarter.CompetitionId = CompetitionId AND CompetitionId IN 
			                                    (SELECT CompetitionId 
			                                      FROM CompetitionInfo 
			                                      WHERE CompetitionInfo.GroupId =@groupId)
		                                    Group by Competition_Quarter.CompetitionId
		                                    )as tmp
                                     JOIN
	                                    (SELECT CompetitionId,CompetitionLocation, CompetitionDate,CompetitionName,Opponent
		                                    FROM CompetitionInfo 
		                                    WHERE GroupId = @groupId) as tmp2
                                     ON tmp.CompetitionId=tmp2.CompetitionId 
                                     ORDER BY CompetitionDate DESC";
                await using (SqlCommand cmd = new SqlCommand(strGetInfo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    await using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                CompetitionIntro competitionIntro = new CompetitionIntro();
                                competitionIntro.CompetitionId = reader.GetInt32(0);
                                competitionIntro.CompetitionDate = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                                competitionIntro.CompetitionLocation = reader.IsDBNull(2)? " no location":reader.GetString(2);
                                competitionIntro.CompetitionName = reader.GetString(3);
                                competitionIntro.OurScore = (int)reader.GetInt64(4);
                                competitionIntro.OpponentScore = reader.GetInt32(5);
                                competitionIntro.OpponentName = reader.GetString(6);
                                competitionIntros.Add(competitionIntro);
                            }
                            stopWacth.Stop();
                            TimeSpan time = stopWacth.Elapsed;
                            Console.WriteLine("時間"+time.TotalMilliseconds.ToString());
                            return CreatedAtAction("GetAllCompetition", new { errorcode = -1, msg = "success get competition introduction", CRecordList = competitionIntros });
                        }
                    }

                }
            }
            return CreatedAtAction("GetAllCompetition", new { errorcode = 404, msg = "this group has no competition record " });
        }
        //GET: api/ShowCompetitionRecord/5/1/player
        //2. 得單一個比賽紀錄的細節(個人)
        [HttpGet("{groupId}/{competitionId}/player")]
        public async Task<ActionResult> GetPersonDetailCompetition([FromRoute]int groupId, int competitionId)
        {
            Stopwatch watchPersonRecord = new Stopwatch();
            watchPersonRecord.Start();
            Console.WriteLine("get Player Record In One Competition" + competitionId);
            List<PlayerRecord> playerRecords = new List<PlayerRecord>();

            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                //得此比賽個人紀錄
                string strGetInfo = @"SELECT tmp.UserName, tmp2.PersonScore,tmp2.IsStart,tmp2.ORB,tmp2.DRB,
                                        tmp2.Assists,tmp2.Steals, tmp2.Blocks,tmp2.FGA,tmp2.FGM,tmp2.FG3,
                                        tmp2.FGM3,tmp2.FTA,tmp2.FTM,tmp2.Turnovers,tmp2.PF FROM
	                                        (SELECT  UserName,UserId
		                                        FROM UserInfo
		                                        WHERE UserInfo.UserId = UserId AND UserId IN 
			                                        (SELECT Competition_User.UserId 
			                                          FROM Competition_User 
			                                          WHERE Competition_User.CompetitionId =@competitionId)
		                                        )as tmp
                                         JOIN
	                                        (SELECT * 
		                                        FROM Competition_User
		                                        WHERE CompetitionId =@competitionId
                                                ) as tmp2
                                         ON tmp.UserId=tmp2.UserId
                                         ORDER BY IsStart DESC";
                await using (SqlCommand cmd = new SqlCommand(strGetInfo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@competitionId", competitionId);
                    await using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                PlayerRecord playerRecord = new PlayerRecord();
                                playerRecord.UserName = reader.GetString(0);
                                playerRecord.PersonScore = reader.GetInt32(1);
                                playerRecord.IsStart = reader.GetBoolean(2);
                                playerRecord.ORB = reader.GetInt32(3);
                                playerRecord.DRB = reader.GetInt32(4);
                                playerRecord.Assists = reader.GetInt32(5);
                                playerRecord.Steals = reader.GetInt32(6);
                                playerRecord.Blocks = reader.GetInt32(7);
                                playerRecord.FGA = reader.GetInt32(8);
                                playerRecord.FGM = reader.GetInt32(9);
                                playerRecord.FG3 = reader.GetInt32(10);
                                playerRecord.FGM3 = reader.GetInt32(11);
                                playerRecord.FTA = reader.GetInt32(12);
                                playerRecord.FTM = reader.GetInt32(13);
                                playerRecord.Turnovers = reader.GetInt32(14);
                                playerRecord.PF = reader.GetInt32(15);
                                playerRecords.Add(playerRecord);
                            }
                            watchPersonRecord.Stop();
                            Console.WriteLine(string.Format("runTime: {0}",watchPersonRecord.ElapsedMilliseconds));
                            return CreatedAtAction("GetPersonDetailCompetition", new { errorcode = -1, msg = "success get personal record", CRecordList = playerRecords });
                        }
                    }
                }
                
            }
            return CreatedAtAction("GetPersonDetailCompetition", new { errorcode = 404, msg = "this competition has no personal record " });
        }

        //GET: api/ShowCompetitionRecord/5/1/quarter
        //3. 得單一個比賽紀錄的細節(quarter)
        [HttpGet("{groupId}/{competitionId}/quarter")]
        public async Task<ActionResult> GetQuarterDetailCompetition([FromRoute]int groupId, int competitionId)
        {
            Console.WriteLine("get Quarter Record In One Competition" + competitionId);
            List<QuarterRecord> quarterRecords = new List<QuarterRecord>();

            await using (SqlConnection sqlConnection = new SqlConnection(Global.connect_string))
            {
                sqlConnection.Open();
                //得此比賽個人紀錄
                string strGetInfo = @"SELECT [Quarter],[QuarterScore],[TF],[OpponentScore]
                                          FROM[Competition_Quarter]
                                          WHERE CompetitionId =@competitionId ";
                await using (SqlCommand cmd = new SqlCommand(strGetInfo, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@competitionId", competitionId);
                    await using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                QuarterRecord quarterRecord = new QuarterRecord();
                                quarterRecord.Quarter = reader.GetInt32(0);
                                quarterRecord.QuarterScore = (int)reader.GetInt64(1);
                                quarterRecord.TF = reader.GetInt32(2);
                                quarterRecord.OpponentScore = reader.GetInt32(3);
                                quarterRecords.Add(quarterRecord);
                            }
                            return CreatedAtAction("GetQuarterDetailCompetition", new { errorcode = -1, msg = "success get quarter record", CRecordList = quarterRecords });
                        }
                    }
                }

            }
            return CreatedAtAction("GetQuarterDetailCompetition", new { errorcode = 404, msg = "this competition has no quarter record " });
        }

    }
}
