using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Models
{
    public class Competition_Quarter
    {
        [Key]
        public int? QuarterRecordId { get; set; }
        public int CompetitionId { get; set; }
        public int Quarter { get; set; }
        public int QuarterScore { get; set; }
        public int TF { get; set; }
        public int OpponentScore { get; set; }

      
    }
}
