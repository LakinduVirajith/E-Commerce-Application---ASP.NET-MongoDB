using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Models;
using E_Commerce_Application___ASP.NET_MongoDB.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace E_Commerce_Application___ASP.NET_MongoDB.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary> registers a new user in the system. </summary>
        // POST: api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister userDto)
        {
            return await _userService.RegisterUser(userDto);
        }
    }
}
