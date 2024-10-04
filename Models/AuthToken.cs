using MongoDB.Bson.Serialization.Attributes;

namespace E_Commerce_Application___ASP.NET_MongoDB.Models
{
    public class AuthToken
    {
        [BsonElement("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [BsonElement("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [BsonElement("device_id")]
        public string DeviceId { get; set; } = string.Empty; // UNIQUE IDENTIFIER FOR THE DEVICE

        [BsonElement("expiry")]
        public DateTime Expiry { get; set; }

        [BsonElement("refresh_token_expiry")]
        public DateTime RefreshTokenExpiry { get; set; }

        [BsonElement("last_used")]
        public DateTime LastUsed { get; set; } // TRACK WHEN THE TOKEN WAS LAST USED
    }
}
