using AutoMapper;
using DocShareApp.Helpers;
using DocShareApp.Models;
using DocShareApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocShareApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public DateTime Expires { get; private set; }

        public UsersController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        //FromBody notation used so parametres are fetched from the HTTP message body
        //framework will search for a json in the HTTP message body so a model can be created
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or Password is incorrect" });
            //return BadRequest("Username or password is incorrect");

            //Generating JWT if authentication is successful(creating token):

            var tokenHandler = new JwtSecurityTokenHandler();
            //Generating key stored in AppSettings to digitally sign the token
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            //The descriptor is the payload of the JWT and can have whatever date the developer
            //wants to insert
            //Editing proprieties
            var tokenDescriptor = new SecurityTokenDescriptor()
            //tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            //creating the token with the specified descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);
            //serializaing the token
            var tokenString = tokenHandler.WriteToken(token);

            //returning basic user info + the token
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }
    }
}



