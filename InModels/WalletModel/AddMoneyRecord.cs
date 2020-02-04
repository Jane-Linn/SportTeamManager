using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.WalletModel
{
    public class AddMoneyRecord
    {
        
        public string RecordDate { get; set; }//時間(傳入樣式yyyy-MM-dd)
        public string WalletDescribe { get; set; }//備註
        public decimal MoneyTrack { get; set; }//入賬還是出支
       
    }
}
