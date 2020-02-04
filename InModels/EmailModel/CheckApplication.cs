using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels
{
    public class CheckApplication
    {
        //未做確認他是manager(因為正常狀況你是manager才會出現這個通知
        public int NotifyId { get; set; }
        public string Status { get; set; }
    }
}
