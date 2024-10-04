namespace E_Commerce_Application___ASP.NET_MongoDB.DTOs
{
    public class UserToken
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}
