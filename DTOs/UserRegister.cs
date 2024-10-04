using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Application___ASP.NET_MongoDB.DTOs
{
    public class UserRegister
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
