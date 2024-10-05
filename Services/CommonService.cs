using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class CommonService : ICommonService
    {
        // METHOD TO HASH PASSWORD USING BCrypt
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // METHOD TO GENERATE A RANDOM TOKEN
        public string GenerateActivationToken()
        {
            return Guid.NewGuid().ToString();
        }

        // METHOD TO VERIFY PASSWORD
        public bool IsVerifyPassword(string storedPassword, string inputPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }

        // METHOD TO VALIDATE A MAC ADDRESS FORMAT.
        public bool IsValidMacAddress(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
            {
                return false;
            }

            string macPattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            return Regex.IsMatch(macAddress, macPattern);
        }

        // METHOD TO VALIDATE ANY DTO AND RETURN THE FIRST ERROR MESSAGE FOUND
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
                return results.First().ErrorMessage;
            }

            return null;
        }

        // GENERIC METHOD FOR MAPPING PROPERTIES FROM ONE OBJECT TO ANOTHER
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
