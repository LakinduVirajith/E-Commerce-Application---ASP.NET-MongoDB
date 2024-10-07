using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface IAuthService
    {
        Task<IActionResult> RegisterAsync(UserRegister registerDto);
        Task<ActionResult<UserAuthToken>> LoginAsync(UserLogin loginDto);
        Task<IActionResult> ActivateAsync(string activationToken);
        Task<ActionResult<UserAuthToken>> RefreshTokenAsync(UserRefreshToken request);
        Task<IActionResult> LogoutAsync(string deviceId);
        Task<IActionResult> SendResetPasswordOtpAsync(string email);
        Task<IActionResult> ValidateOtpAsync(UserValidateOtp validateOtpDto);
        Task<IActionResult> ResetPasswordAsync(UserResetPassword resetPasswordDto);
        Task<IActionResult> ChangeEmailAsync(UserChangeEmail changeEmailDto);
        Task<IActionResult> ChangePasswordAsync(UserChangePassword changePasswordDto);
    }
}
