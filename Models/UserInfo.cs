using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Models
{
    public class UserInfo
    {
        [Key]
        public int? UserId { get; set;}
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string? UserPhoto { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public int Gender { get; set; }
        public string Birthday { get; set; }
    }
}
