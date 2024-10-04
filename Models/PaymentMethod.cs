using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_Commerce_Application___ASP.NET_MongoDB.Models
{
    public class PaymentMethod
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // Unique ID for the payment method

        [BsonElement("card_number")]
        public string CardNumber { get; set; } // Encrypted or masked card number

        [BsonElement("cardholder_name")]
        public string CardholderName { get; set; } // Name on the card

        [BsonElement("expiry_date")]
        public DateTime ExpiryDate { get; set; } // Expiry date of the card

        [BsonElement("is_default")]
        public bool IsDefault { get; set; } // Is this the default payment method?
    }
}
