using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.GameRecordModel
{
    public class CompetitionIntro
    {
        //需要傳我方的球隊名嗎???
        public int CompetitionId { get; set; }
       
        public string CompetitionDate { get; set; }
        public string CompetitionName { get; set; }
        public string CompetitionLocation { get; set; }
        public string OpponentName { get; set; }
        public int OurScore { get; set; }
        public int OpponentScore { get; set; }
    }
}
