using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels
{
    public class AddGroupFromApp
    {
        public string Token { get; set; }
        public string GroupName { get; set; }
      
        public string Age { get; set; }
        public int WinTime { get; set; }
        public int LoseTime { get; set; }
        public string InvitedCode { get; set; }

        public string GroupIntro { get; set; }
    }
}
