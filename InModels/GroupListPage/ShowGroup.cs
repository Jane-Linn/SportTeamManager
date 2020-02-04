using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels
{
    public class ShowGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupPhoto { get; set; }
        public string GroupIntro { get; set; }
        public string NextEvent { get; set; }
        public string InvitedCode { get; set; }

    }
}
