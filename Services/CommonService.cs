using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class CommonService
    {
        // 1. METHOD TO HASH PASSWORD USING BCrypt
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // 2. GENERIC METHOD TO VALIDATE ANY DTO
        public string? ValidateDto<T>(T dto)
        {
            if (dto == null)
            {
                return "DTO cannot be null.";
            }

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, context, results, true);
            if (!isValid)
            {
                // RETURN THE FIRST ERROR MESSAGE FOUND
                return results.First().ErrorMessage;
            }

            return null; // NO VALIDATION ERRORS
        }

        // 3. GENERIC METHOD FOR MAPPING PROPERTIES FROM ONE OBJECT TO ANOTHER
        public void MapProperties<TSource, TDestination>(TSource source, TDestination destination)
        {
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = typeof(TDestination).GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.FirstOrDefault(dp => dp.Name == sourceProperty.Name && dp.CanWrite);
                if (destinationProperty != null)
                {
                    var value = sourceProperty.GetValue(source);
                    destinationProperty.SetValue(destination, value);
                }
            }
        }
    }
}
