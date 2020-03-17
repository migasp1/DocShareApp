using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocShareApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocShareApp.Controllers
{
[Authorize]
[Route("[controler]")]
[ApiController]
public class UsersController : ControllerBase
{
private IUserService userService;



}
}