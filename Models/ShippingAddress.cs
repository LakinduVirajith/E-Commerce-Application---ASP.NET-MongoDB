using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_Commerce_Application___ASP.NET_MongoDB.Models
{
    public class ShippingAddress
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // Unique ID for the address

        [BsonElement("recipient_name")]
        public string RecipientName { get; set; } // Name of the recipient

        [BsonElement("address_line1")]
        public string AddressLine1 { get; set; } // Street address

        [BsonElement("address_line2")]
        public string AddressLine2 { get; set; } // Apartment, suite, etc. (optional)

        [BsonElement("city")]
        public string City { get; set; } // City

        [BsonElement("state")]
        public string State { get; set; } // State or region

        [BsonElement("postal_code")]
        public string PostalCode { get; set; } // Zip or postal code

        [BsonElement("country")]
        public string Country { get; set; } // Country

        [BsonElement("is_default")]
        public bool IsDefault { get; set; } // Is this the default address?
    }
}
