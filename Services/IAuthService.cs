using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public interface IAuthService
    {
        Task<IActionResult> RegisterUser(UserRegister userDto);
        Task<ActionResult<UserAuthToken>> LoginUser(UserLogin loginDto);
        Task<IActionResult> ActivateUser(string activationToken);
        Task<ActionResult<UserAuthToken>> RefreshToken(UserRefreshToken request);
        Task<IActionResult> LogoutUser();
    }
}
