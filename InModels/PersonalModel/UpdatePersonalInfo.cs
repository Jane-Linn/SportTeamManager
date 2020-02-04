using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.PersonalModel
{
    public class UpdatePersonalInfo
    {
        //照片另外傳
       public string Token { get; set; }
        public string UserName { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
    }
}
