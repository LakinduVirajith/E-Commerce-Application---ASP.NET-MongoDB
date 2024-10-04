using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using E_Commerce_Application___ASP.NET_MongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly CommonService _commonService;
        private readonly TokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(MongoDbService mongoDbService, CommonService commonService, TokenService tokenService, JwtSettings jwtSettings)
        {
            _usersCollection = mongoDbService.GetCollection<User>("user");
            _commonService = commonService;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
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
                if (!Enum.IsDefined(typeof(UserRole), validationDto.Role))
                {
                    return new BadRequestObjectResult("Invalid role. Please provide a valid role.");
                }

                // MAP VALIDATION DTO TO A USER MODEL
                var user = new User();
                _commonService.MapProperties(validationDto, user);
                user.Password = _commonService.HashPassword(userDto.Password); // HASH THE PASSWORD
                user.IsActive = false;

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

                // GENERATE ACTIVATION TOKEN AND SET EXPIRY (10 MINUTES)
                user.ActivationToken.Token = _commonService.GenerateActivationToken();
                user.ActivationToken.Expiry = DateTime.UtcNow.AddMinutes(10);                

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
            // FIND THE USER BY USERNAME OR EMAIL
            var user = await _usersCollection.Find(u => u.Username == loginDto.Username || u.Email == loginDto.Username).FirstOrDefaultAsync();

            // CHECK IF THE USER EXISTS AND VALIDATE PASSWORD
            if (user == null || !_commonService.VerifyPassword(user.Password, loginDto.Password))
            {
                return new BadRequestObjectResult("Invalid username or password.");
            }

            // CHECK IF THE USER IS ACTIVATED
            if (!user.IsActive)
            {
                return new BadRequestObjectResult("Your account is not activated. Please activate your account before logging in.");
            }

            // GENERATE JWT AND REFRESH TOKEN
            var token = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            // CHECK IF DEVICE ID ALREADY EXISTS
            var existingAuthToken = user.AuthTokens.FirstOrDefault(t => t.DeviceId == loginDto.deviceId);
            if (existingAuthToken != null)
            {
                // UPDATE EXISTING AUTH TOKEN
                existingAuthToken.AccessToken = token;
                existingAuthToken.RefreshToken = refreshToken;
                existingAuthToken.Expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes);
                existingAuthToken.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
                existingAuthToken.LastUsed = DateTime.UtcNow;
            }
            else
            {
                // CREATE A NEW AUTH TOKEN IF DEVICE ID DOES NOT EXIST
                var authToken = new AuthToken
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    DeviceId = loginDto.deviceId,
                    Expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
                    LastUsed = DateTime.UtcNow
                };

                user.AuthTokens.Add(authToken);
            }

            // UPDATE THE USER IN THE DATABASE
            await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            // RETURN THE TOKENS
            var authTokenResponse = new UserAuthToken
            {
                AccessToken = token,
                RefreshToken = refreshToken,
            };

            return new OkObjectResult(authTokenResponse);
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
                return new ObjectResult("An error occurred while activating the user. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<ActionResult<UserAuthToken>> RefreshToken(UserRefreshToken request)
        {
            try
            {
                // FIND THE USER BY THE REFRESH TOKEN
                var user = await _usersCollection.Find(u => u.AuthTokens.Any(t => t.RefreshToken == request.RefreshToken && t.DeviceId == request.DeviceId)).FirstOrDefaultAsync();

                // CHECK IF USER EXISTS AND IF REFRESH TOKEN IS VALID
                if (user == null)
                {
                    return new BadRequestObjectResult("Invalid refresh token.");
                }

                // FIND THE AUTH TOKEN BY DEVICE ID
                var authToken = user.AuthTokens.FirstOrDefault(t => t.RefreshToken == request.RefreshToken && t.DeviceId == request.DeviceId);
                if (authToken == null || authToken.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return new BadRequestObjectResult("Refresh token is invalid or expired.");
                }

                // GENERATE NEW ACCESS TOKEN AND REFRESH TOKEN
                var newToken = _tokenService.GenerateToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken(user);

                // UPDATE THE AUTH TOKEN WITH NEW TOKENS
                authToken.AccessToken = newToken;
                authToken.RefreshToken = newRefreshToken;
                authToken.Expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes);
                authToken.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);
                authToken.LastUsed = DateTime.UtcNow;

                // UPDATE USER IN THE DATABASE
                await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

                // RETURN THE NEW TOKENS
                var authTokenResponse = new UserAuthToken
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                };

                return new OkObjectResult(authTokenResponse);
            }
            catch (Exception)
            {
                // RETURN A FRIENDLY SERVER ERROR MESSAGE
                return new ObjectResult("An error occurred while refreshing the token. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<IActionResult> LogoutUser(string deviceId)
        {
            try
            {
                // FIND THE USER BY DEVICE ID
                var user = await _usersCollection.Find(u => u.AuthTokens.Any(t => t.DeviceId == deviceId)).FirstOrDefaultAsync();

                // CHECK IF USER EXISTS
                if (user == null)
                {
                    return new BadRequestObjectResult("User not found.");
                }

                // REMOVE THE AUTH TOKEN FOR THE SPECIFIED DEVICE ID
                var update = Builders<User>.Update.PullFilter(u => u.AuthTokens, t => t.DeviceId == deviceId);

                // UPDATE USER DOCUMENT IN THE DATABASE
                await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
                return new OkObjectResult("User logged out successfully!");
            }
            catch (Exception)
            {
                // RETURN A FRIENDLY SERVER ERROR MESSAGE
                return new ObjectResult("An error occurred while logging out. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
