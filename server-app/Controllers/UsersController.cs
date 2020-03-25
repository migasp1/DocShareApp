
using DocShareApp.Entities;
using DocShareApp.Helpers;
using DocShareApp.Mapper;
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
        private readonly AppSettings _appSettings;
        private IUserMapper _userMapper;

        public DateTime Expires { get; private set; }

        public UsersController(IUserService userService, IOptions<AppSettings> appSettings, IUserMapper userMapper)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
            _userMapper = userMapper;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        //FromBody notation used so parametres are fetched from the HTTP message body
        //framework will search for a json in the HTTP message body so a model can be created
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);
            //return BadRequest("Username or password is incorrect");

            if (user == null)
                return BadRequest(new { message = "Username or Password is incorrect" });

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
            string tokenString = tokenHandler.WriteToken(token);

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

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            try
            {
                _userService.Create(model);
                return Ok(new { message = "Successfully registred" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
    }
}



