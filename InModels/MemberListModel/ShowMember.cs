using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.MemberListModel
{
    public class ShowMember
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int? UniformNumber { get; set; }
        public string Role { get; set; }
        public string? LastGameDate { get; set; }
        public string UserPhoto { get; set; }
        //出席率
        //出手得分率
        //累積分數

    }
}
