using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppTest.InModels.PersonalModel
{
    public class RegisterInput
    {
       
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public IFormFile UserPhoto { get; set; }
    }
}
