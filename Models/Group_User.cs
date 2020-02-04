using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Models
{
    public class Group_User
    {
        [Key]
        public int? Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int IsManager { get; set; }
        public int Accepted { get; set; }
        public int UniformNumber { get; set; }
        public DateTime LastGameDate { get; set; }
    }
}
