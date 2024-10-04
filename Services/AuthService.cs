using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using E_Commerce_Application___ASP.NET_MongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly CommonService _commonService;

        public AuthService(MongoDbService mongoDbService, CommonService commonService)
        {
            _usersCollection = mongoDbService.GetCollection<User>("user");
            _commonService = commonService;
        }     

        // 1. METHOD TO REGISTER A NEW USER
        public async Task<IActionResult> RegisterUser(UserRegister userDto)
        {
            try
            {
                // MAP DTO TO VALIDATION DTO
                var validationDto = new UserValidation();
                _commonService.MapProperties(userDto, validationDto);

                // VALIDATE DTO PROPERTIES
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

                // MAP VALIDATION DTO TO A USER MODEL
                var user = new User();
                _commonService.MapProperties(validationDto, user);
                user.Password = _commonService.HashPassword(userDto.Password); // HASH THE PASSWORD
                user.IsActive = false;

                // GENERATE ACTIVATION TOKEN AND SET EXPIRY (10 MINUTES)
                user.ActivationToken.Token = _commonService.GenerateActivationToken();
                user.ActivationToken.Expiry = DateTime.UtcNow.AddMinutes(10);

                // CHECK IF A USER WITH THE SAME EMAIL OR USERNAME ALREADY EXISTS
                var existingUserByEmail = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
                var existingUserByUsername = await _usersCollection.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
                if (existingUserByEmail != null)
                {
                    return new ConflictObjectResult("Email is already registered.");
                }
                if (existingUserByUsername != null)
                {
                    return new ConflictObjectResult("Username is already taken.");
                }

                // INSERT THE NEW USER INTO THE COLLECTION
                await _usersCollection.InsertOneAsync(user);
                return new OkObjectResult("User registered successfully! Please check your email to activate your account.");
            }
            catch (Exception)
            {
                // RETURN A FRIENDLY SERVER ERROR MESSAGE
                return new ObjectResult("An error occurred while saving the user. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 2. METHOD TO LOGIN A USER
        public async Task<ActionResult<UserAuthToken>> LoginUser(UserLogin loginDto)
        {
            throw new NotImplementedException();
        }

        // 3. METHOD TO ACTIVATE A USER ACCOUNT
        public async Task<IActionResult> ActivateUser(string activationToken)
        {
            try
            {
                // FIND THE USER BY ACTIVATION TOKEN
                var user = await _usersCollection.Find(u => u.ActivationToken.Token == activationToken).FirstOrDefaultAsync();

                // CHECK IF USER EXISTS AND IF TOKEN IS VALID (NOT EXPIRED)
                if (user == null || user.ActivationToken.Expiry < DateTime.UtcNow)
                {
                    return new BadRequestObjectResult("Invalid or expired activation token.");
                }

                // UPDATE USER AS ACTIVE AND CLEAR ACTIVATION TOKEN
                var update = Builders<User>.Update
                    .Set(u => u.IsActive, true)
                    .Unset(u => u.ActivationToken);

                // UPDATE USER DOCUMENT IN THE DATABASE
                await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
                return new OkObjectResult("User activated successfully!");
            }
            catch (Exception)
            {
                // RETURN A FRIENDLY SERVER ERROR MESSAGE
                return new ObjectResult("An error occurred while saving the user. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<ActionResult<UserAuthToken>> RefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> LogoutUser()
        {
            throw new NotImplementedException();
        }
    }
}
