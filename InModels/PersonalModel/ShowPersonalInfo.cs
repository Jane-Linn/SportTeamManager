using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.PersonalModel
{
    public class ShowPersonalInfo
    {
       
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? UserPhoto { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
        //20200130加生涯積分 和投球命中率
        public int CareerPoint { get; set; }
        public double FieldGoal { get; set; }
    }
}
