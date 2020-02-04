using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Controllers
{
    public static class Global
    {
        private static bool onServer = true;

        public static string connect_string;
        public static string group_photo_url;
        public static string group_photo_route;
        public static string user_photo_url;
        public static string user_photo_route;
        public static void SetEnv()
        {
            if (onServer)
            {
                group_photo_route = "/home/newhappytea241630/UploadFolder/";
                group_photo_url = "http://34.80.133.188/api/Group/";
                user_photo_route = "/home/newhappytea241630/UserPhotoFolder/";
                user_photo_url = "http://34.80.133.188/api/user_photo/";
                connect_string = "Persist Security Info=FALSE;User ID=JANE;Password=Ji241630;Initial Catalog=test;Server=nettest";
            }
            else
            {
                group_photo_route = "D:/UploadFolder/";
                group_photo_url = "http://localhost:5000/api/Group/";
                user_photo_route = "D:/UserPhoto/";
                user_photo_url = "http://localhost:5000/api/user_photo/";
                connect_string = "Data Source=LAPTOP-DUD76Q5O;Initial Catalog=sportteam;Integrated Security=True";
            }
        }


    }
}
