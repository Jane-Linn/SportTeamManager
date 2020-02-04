using System;
using System.ComponentModel.DataAnnotations;

namespace AppTest.Models
{
    public class WalletInfo
    {
        [Key]
        public int? WalletRecordId { get; set; }
        
        public decimal MoneyTrack { get; set; }
       
        public string WalletDescribe { get; set; }

        public DateTime RecordDate { get; set; }
        public int GroupId { get; set; }

    }
}