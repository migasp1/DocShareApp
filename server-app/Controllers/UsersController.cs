
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
using System.Threading;
using System.Threading.Tasks;

namespace DocShareApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private IUserService _userService;
        private readonly KeyOptions _appSettings;
  
        public UsersController(IUserService userService, IOptions<KeyOptions> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        //FromBody notation used so parametres are fetched from the HTTP message body
        //framework will search for a json in the HTTP message body so a model can be created
        //cancellation token is optional (default)
        public async Task<IActionResult> Authenticate([FromBody]AuthenticateModel model, CancellationToken cancellationToken = default)
        {
            User user = await _userService.Authenticate(model.Email, model.Password, cancellationToken).ConfigureAwait(false);
     
            if (user == null)
                return BadRequest(new { message = "Email or Password is incorrect" });

            //Generating JWT if authentication is successful(creating token):
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //Generating key stored in AppSettings to digitally sign the token
            byte[] key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            //The descriptor is the payload of the JWT and can have whatever date the developer
            //wants to insert
            //Editing proprieties
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
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
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
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
        public async Task<IActionResult> Register([FromBody]RegisterModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userService.Create(model, cancellationToken).ConfigureAwait(false);
                return Ok(new { message = "Successfully registered" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("userPersonalInfo")]
        public async Task<IActionResult> RetrievePersonalUserInfo(CancellationToken cancellationToken = default)
        {
            try
            {
                User user = await _userService.RetrievePersonalUserInfo(int.Parse(HttpContext.User.Identity.Name), cancellationToken).ConfigureAwait(false);
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
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetAllUsers(cancellationToken).ConfigureAwait(false);

            return Ok(users);
        }


        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            var id = HttpContext.User.Identity.Name;
            try
            {
                await _userService.ChangePassword(model, int.Parse(id), cancellationToken).ConfigureAwait(false);
                return Ok(new { message = "Changed password successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Something went wrong: ERROR " + ex.Message });
            }
        }

        [HttpPost("changeNameUser")]
        public async Task<IActionResult> ChangeNameUser([FromBody]ChangeNameUserModel model, CancellationToken cancellationToken = default)
        {
            var id = HttpContext.User.Identity.Name;
            try
            {
                await _userService.ChangeNamesUser(model, int.Parse(id), cancellationToken).ConfigureAwait(false);
                return Ok(new { message = "Changed Name successfuly" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Something went wrong: ERROR " + ex.Message });

            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (id == int.Parse(HttpContext.User.Identity.Name))
                    return Forbid("Cannot delete current user");
                await _userService.Delete(id, cancellationToken).ConfigureAwait(false);
                return Ok(new { message = "User successfully deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Something went wrong ERROR: {ex.Message}" });
            }
        }
    }
}



