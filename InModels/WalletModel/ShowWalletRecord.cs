using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.WalletModel
{
    public class ShowWalletRecord
    {
        public int WalletRecordId { get; set; }//要修改就回傳這個
        public string RecordDate { get; set; }//時間(會照時間排序)
        public string WalletDescribe { get; set; }//備註
        public decimal MoneyTrack { get; set; }//入賬還是出支
        public decimal TotalMoney { get; set; }//每一筆的結餘

    }
}
