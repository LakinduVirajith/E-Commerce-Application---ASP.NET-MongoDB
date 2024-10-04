using MongoDB.Bson.Serialization.Attributes;

namespace E_Commerce_Application___ASP.NET_MongoDB.Models
{
    public class ActivationToken
    {
        [BsonElement("token")]
        public string Token { get; set; } = string.Empty;

        [BsonElement("expiry")]
        public DateTime Expiry { get; set; }
    }
}
