using E_Commerce_Application___ASP.NET_MongoDB.Models;

namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken(User user);
    }
}
