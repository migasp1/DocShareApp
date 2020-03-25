using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocShareApp.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class DocShareAppController : ControllerBase
    {
        //POST: api/DocShareApp/Login

        [HttpPost("Login")]
        public IActionResult Teste()
        {
            return Ok(new { message = "Foste retornado Mihail" }); 
        }


    }
}
