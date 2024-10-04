using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using System;

namespace E_Commerce_Application___ASP.NET_MongoDB.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;  // MONGODB WILL ASSIGN A UNIQUE OBJECTID

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("role")]
        public String Role { get; set; } = string.Empty;

        [BsonElement("activation_token")]
        public ActivationToken ActivationToken { get; set; } = new ActivationToken();

        [BsonElement("shipping_addresses")]
        public List<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();

        [BsonElement("payment_methods")]
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("is_active")]
        public bool IsActive { get; set; } = false;
    }
}
