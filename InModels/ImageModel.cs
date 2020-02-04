using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels
{
    public class ImageModel
    {
        public int GroupId { get; set; }
        public string Token { get; set; }
        public IFormFile Photo { get; set; }
    }
}
