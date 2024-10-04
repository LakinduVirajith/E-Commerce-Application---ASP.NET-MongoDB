namespace E_Commerce_Application___ASP.NET_MongoDB.Helpers
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double TokenExpiryInMinutes { get; set; }
        public double RefreshTokenExpiryInDays { get; set; }
    }
}
