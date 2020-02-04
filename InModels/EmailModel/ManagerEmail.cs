using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels
{
    public class ManagerEmail
    {
        public int NotifyId { get; set;}
        public int RequestId { get; set; }//之後可以直接點名字看個人資料?
        public string RequestName { get; set; }
      
        public string GroupName { get; set; }
    }
}
