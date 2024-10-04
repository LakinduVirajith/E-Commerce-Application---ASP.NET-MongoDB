using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public interface IUserService
    {
        Task<IActionResult> RegisterUser(UserRegister userDto);
    }
}
