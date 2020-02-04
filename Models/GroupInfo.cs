using System.ComponentModel.DataAnnotations;

namespace AppTest.Models
{
    public class GroupInfo
    {
        [Key]
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupPhoto { get; set; }
        public int WalletId { get; set; }
        public int Age { get; set; }
        public int WinTime { get; set; }
        public int LoseTime{ get; set; }
        public string InvitedCode { get; set; }

        public string GroupIntro { get; set; }

    }
}