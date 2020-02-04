using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTest.CreateFakeData
{
    public class CreateUser
    {
        public const int Number = 100;

        public static List<User> users;
        public CreateUser()
        {
            users = new List<User>();
            create();
        }

        private void create()
        {
            var strLast = "王李張劉陳楊黃吳趙周徐孫馬朱胡林郭何高羅鄭梁謝宋唐許鄧馮韓曹曾彭蕭蔡潘田董袁於余葉蔣杜蘇魏程呂丁沈任姚盧傅鍾姜崔譚廖范汪陸金石戴賈夏邱方侯鄒熊孟秦白江閻薛尹段雷黎史龍陶賀顧毛郝龔邵萬錢嚴賴覃洪武莫孔";
            var strFirst = "世舜丞主產仁仇倉仕仞任伋眾伸佐佺侃儕促俟信俁修倝倡倧償儲僖僧僳儒俊偉列則剛創前劍助劭勢勘參叔吏嗣士壯孺守寬賓宋宗宙宣實宰尊峙峻崇崈川州巡帥才承拯操齋昌晁暠曹曾珺瑋珹琒琛琩琮琸瑎瑒璟璥瑜生疇矗矢石磊砂碫示社祖祚祥禪稹穆竣竦綜縝緒艙舷船蚩襦軾輯軒子傑榜碧葆萊蒲天樂東鋼鐸鋮鎧鑄鏗鋒鎮鍵鐮馗旭駿驄驥駒駕驕誠諍賜慕端征堅建弓強彥御悍擎攀曠昂晷健冀凱劻嘯柴木林森朴騫寒函高魁魏鮫鯤鷹丕乒候冕勰備憲賓密封山峰弼彪彭旁日明昪昴勝漢涵汗浩淏清瀾浦澉澎澔濮濯瀚瀛灝滄虛豪豹輔輩邁邶合部闊雄霆震韓俯頒頗頻頷風颯飆飈馬亮侖仝代儋利力劼勒卓哲喆展帝弛弢弩彰征律德志忠思振挺掣旲旻昊昮晉晟晸朗段殿泰滕炅煒煜煊炎選玄勇君稼黎利賢誼金鑫輝墨歐有友聞問";
            var next = new Random();
            //創Number個使用者
            for (int j = 0; j < Number; j++)
            {
                User oneUser = new User();
                //創名字
                var builder = new StringBuilder();
                builder.Append(strLast[next.Next(0, strLast.Length)]);
                for (var i = 0; i < 2; i++)
                {
                    builder.Append(strFirst[next.Next(0, strFirst.Length)]);
                }
                string name = builder.ToString();

                oneUser.Name = name;

                //創email
                var strEmail = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                var buildEmail = new StringBuilder();
                for (int m = 0; m < 7; m++)
                {
                    buildEmail.Append(strEmail[next.Next(0, strEmail.Length)]);
                }
                buildEmail.Append("@gmail.com");
                oneUser.Email = buildEmail.ToString();
                //加password
                oneUser.Password = "1234";
                users.Add(oneUser);

            }
        }

    }
}
