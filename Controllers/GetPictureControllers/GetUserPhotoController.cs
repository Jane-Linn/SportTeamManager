using AppTest.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.Controllers.GetPictureControllers
{
    [Route("api/user_photo")]
    [ApiController]
    public class GetUserPhotoController : ControllerBase
    {
        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"}
        };
        private readonly string _folder;
        private readonly UserContext _context;
        public GetUserPhotoController(UserContext context)
        {
            // 把上傳目錄設為：wwwroot\UploadFolder
            _folder = Global.user_photo_route;
            _context = context;
            Console.WriteLine("%吃路徑 " + _folder);
        }
        //*取得圖片的api
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {

            Console.WriteLine("Download user pic");
            if (string.IsNullOrEmpty(fileName))
            {
                return CreatedAtAction("Download", new { errorcode = 401, msg = "no found photo" });
            }

            var path = $"{_folder}{fileName}";
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            // 回傳檔案到 Client 需要附上 Content Type，否則瀏覽器會解析失敗。
            return new FileStreamResult(memoryStream, _contentTypes[Path.GetExtension(path).ToLowerInvariant()]);
        }
    }
}
