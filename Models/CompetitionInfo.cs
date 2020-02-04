using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Models
{
    public class CompetitionInfo
    {
        [Key]
        public int? CompetitionId { get; set; }
        public int GroupId{get; set; }
        public string CompetitionName { get; set; }
        public int TotalScore { get; set; }
        public string CompetitionDate { get; set; }
        public string CompetitionDescribe { get; set; }
        public string Opponent { get; set; }
        public int Status { get; set; }

    }
}
