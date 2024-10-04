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
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary> registers a new user in the system. </summary>
        // POST: api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister userDto)
        {
            return await _authService.RegisterUser(userDto);
        }

        /// <summary> logs in an existing user.</summary>
        // POST: api/v1/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<UserAuthToken>> Login([FromBody] UserLogin loginDto)
        {
            return await _authService.LoginUser(loginDto);
        }

        /// <summary> activates a registered user (email confirmation). </summary>
        // PUT: api/v1/auth/activate
        [HttpPut("activate")]
        public async Task<IActionResult> Activate([FromQuery] string activationToken)
        {
            return await _authService.ActivateUser(activationToken);
        }

        /// <summary> refreshes the access token using a valid refresh token. </summary>
        // PUT: api/v1/auth/refresh-token
        [HttpPut("refresh-token")]
        public async Task<ActionResult<UserAuthToken>> RefreshToken([FromBody] UserRefreshToken request)
        {
            return await _authService.RefreshToken(request);
        }

        /// <summary> log-out the user by invalidating the tokens. </summary>
        // GET: api/v1/auth/logout
        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] string deviceId)
        {
            return await _authService.LogoutUser(deviceId);
        }
    }
}
