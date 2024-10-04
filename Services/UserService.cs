using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using E_Commerce_Application___ASP.NET_MongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly CommonService _commonService;

        public UserService(MongoDbService mongoDbService, CommonService commonService)
        {
            _usersCollection = mongoDbService.GetCollection<User>("user");
            _commonService = commonService;
        }

        // 1. METHOD TO REGISTER A NEW USER
        public async Task<IActionResult> RegisterUser(UserRegister userDto)
        {
            // MAP DTO TO A VALIDATION DTO
            var validationDto = new UserValidation();
            _commonService.MapProperties(userDto, validationDto);

            // VALIDATE THE DTO PROPERTIES
            var validationError = _commonService.ValidateDto(validationDto);
            if (validationError != null)
            {
                return new BadRequestObjectResult(validationError);
            }

            // CHECK IF THE ROLE IS VALID
            if (!Enum.IsDefined(typeof(UserRole), userDto.Role))
            {
                return new BadRequestObjectResult("Invalid role. Please provide a valid role.");
            }

            // MAP VALIDATION DTO TO A MODEL
            var user = new User();
            _commonService.MapProperties(validationDto, user);
            user.Password = _commonService.HashPassword(userDto.Password); // HASH THE PASSWORD
            user.IsActive = false;

            // CHECK IF USER EXISTS BY EMAIL OR USERNAME
            var existingUserByEmail = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            var existingUserByUsername = await _usersCollection.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUserByEmail != null || existingUserByUsername != null)
            {
                return new ConflictObjectResult("User with this email or username already exists.");
            }

            // INSERT THE NEW USER INTO THE COLLECTION
            await _usersCollection.InsertOneAsync(user);
            return new OkObjectResult("User registered successfully!");
        }
    }
}
