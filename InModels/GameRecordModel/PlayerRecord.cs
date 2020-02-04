using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.GameRecordModel
{
    public class PlayerRecord
    {
        public string UserName { get; set; }
        public int PersonScore { get; set; }
        public bool IsStart { get; set; }
        public int ORB { get; set; }
        public int DRB { get; set; }
        public int Assists { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public int FGA { get; set; }
        public int FGM { get; set; }
        public int FG3 { get; set; }
        public int FGM3 { get; set; }
        public int FTA { get; set; }
        public int FTM { get; set; }
        public int Turnovers { get; set; }
        public int PF { get; set; }


    }
}
