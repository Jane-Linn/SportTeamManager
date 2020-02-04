using AppTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.GameRecordModel
{
    public class GameRecordFromApp
    {
        public int GroupId { get; set; }
        public string CompetitionName { get; set; }
        public int TotalScore { get; set; }
        public string CompetitionDate { get; set; }
        public string CompetitionDescribe { get; set; }
        public string Opponent { get; set; }
        public int Status { get; set; }
       public string CompetitionLocation { get; set; }

        public List<PlayerRecordFromApp> Players { get; set; }

        public List<Competition_Quarter> Quarters { get; set; }
    }
}
