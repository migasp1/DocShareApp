
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
            var user = _userService.Authenticate(model.Email, model.Password);
            //return BadRequest("Email or password is incorrect");

            if (user == null)
                return BadRequest(new { message = "Email or Password is incorrect" });

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
                    new Claim(ClaimTypes.Name, user.Id.ToString()),

                    //Role is sent in the token
                    new Claim(ClaimTypes.Role, user.Role)
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
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
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

        [HttpGet("userPersonalInfo")]
        public IActionResult RetrievePersonalUserInfo()
        {
            try
            {
                User user = _userService.RetrievePersonalUserInfo(int.Parse(HttpContext.User.Identity.Name));
                return Ok(new
                {
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Something went wrong ERROR: {ex.Message}" });
            }
        }


        [Authorize(Roles = Role.Admin)]
        [HttpGet("getAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }


        [HttpPost("changePassword")]
        public IActionResult ChangeUserPassword([FromBody] ChangePasswordModel model)
        {
            var id = HttpContext.User.Identity.Name;
            try
            {
                _userService.ChangePassword(model, int.Parse(id));
                return Ok(new { message = "Changed password successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Something went wrong: ERROR " + ex.Message });
            }
        }

        [HttpPost("changeNameUser")]
        public IActionResult ChangeNameUser([FromBody]ChangeNameUserModel model)
        {
            var id = HttpContext.User.Identity.Name;
            try
            {
                _userService.ChangeNamesUser(model, int.Parse(id));
                return Ok(new { message = "Changed Name successfuly" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Something went wrong: ERROR " + ex.Message });

            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id == int.Parse(HttpContext.User.Identity.Name))
                    return Forbid("Cannot delete current user");
                _userService.Delete(id);
                return Ok(new { message = "User successfully deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Something went wrong ERROR: {ex.Message}" });
            }
        }
    }
}



