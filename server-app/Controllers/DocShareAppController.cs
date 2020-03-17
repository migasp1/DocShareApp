using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocShareApp.Controllers
{
    [Route("api")]
    [ApiController]
    public class DocShareAppController : ControllerBase
    {
        //POST: api/DocShareApp/Login
        [HttpPost("Login")]
        public String Put(string useraname, string password)
        {
            return "Foste loggado, Mihai!";
        }


    }
}
