using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Register([FromBody] UserRegister registerDto)
        {
            return await _authService.RegisterAsync(registerDto);
        }

        /// <summary> logs in an existing user.</summary>
        // POST: api/v1/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<UserAuthToken>> Login([FromBody] UserLogin loginDto)
        {
            return await _authService.LoginAsync(loginDto);
        }

        /// <summary> activates a registered user (email confirmation). </summary>
        // GET: api/v1/auth/activate?token={token}
        [HttpGet("activate")]
        public async Task<IActionResult> Activate([FromQuery] string token)
        {
            return await _authService.ActivateAsync(token);
        }

        /// <summary> refreshes the access token using a valid refresh token. </summary>
        // PUT: api/v1/auth/refresh-token
        [HttpPut("refresh-token")]
        public async Task<ActionResult<UserAuthToken>> RefreshToken([FromBody] UserRefreshToken request)
        {
            return await _authService.RefreshTokenAsync(request);
        }

        /// <summary> log-out the user by invalidating the tokens. </summary>
        // PUT: api/v1/auth/logout
        [HttpPut("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] string deviceId)
        {
            return await _authService.LogoutAsync(deviceId);
        }

        /// <summary> Sends an OTP for resetting the password. </summary>
        // POST: api/v1/auth/send-reset-password-otp
        [HttpPost("send-reset-password-otp")]
        public async Task<IActionResult> SendResetPasswordOtp([FromBody] string email)
        {
            return await _authService.SendResetPasswordOtpAsync(email);
        }

        /// <summary> Validates the OTP for resetting the password. </summary>
        // POST: api/v1/auth/validate-otp
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] UserValidateOtp validateOtpDto)
        {
            return await _authService.ValidateOtpAsync(validateOtpDto);
        }

        /// <summary> Resets the password using a valid OTP. </summary>
        // POST: api/v1/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPassword resetPasswordDto)
        {
            return await _authService.ResetPasswordAsync(resetPasswordDto);
        }

        /// <summary> Changes the user's email when logged in. </summary>
        // PUT: api/v1/auth/change-email
        [HttpPut("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] UserChangeEmail changeEmailDto)
        {
            return await _authService.ChangeEmailAsync(changeEmailDto);
        }

        /// <summary> Changes the user's password when logged in. </summary>
        // PUT: api/v1/auth/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePassword changePasswordDto)
        {
            return await _authService.ChangePasswordAsync(changePasswordDto);
        }
    }
}
