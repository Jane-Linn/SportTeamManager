using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Models
{
    public class Competition_User
    {
        [Key]
        public int? RecordId { get; set; }
        public int UserId { get; set; }
        public int CompetitionId { get; set; }//對照Competition_Quarter的表
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
        public int PersonScore { get; set; }
        public bool IsStart { get; set; }
    }
}
