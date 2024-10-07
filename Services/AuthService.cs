using E_Commerce_Application___ASP.NET_MongoDB.DTOs;
using E_Commerce_Application___ASP.NET_MongoDB.Enums;
using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using E_Commerce_Application___ASP.NET_MongoDB.Interfaces;
using E_Commerce_Application___ASP.NET_MongoDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService _mailService;
        private readonly ICommonService _commonService;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IMongoDbService mongoDbService,
            IHttpContextAccessor httpContextAccessor,
            IMailService mailService,
            ICommonService commonService, 
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _usersCollection = mongoDbService.GetCollection<User>("user");
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _commonService = commonService;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }     

        // 1. METHOD TO REGISTER A NEW USER
        public async Task<IActionResult> RegisterAsync(UserRegister registerDto)
        {
            try
            {
                // MAP DTO TO VALIDATION DTO
                var validationDto = new UserValidation();
                _commonService.MapProperties(registerDto, validationDto);

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
                user.Password = _commonService.HashPassword(validationDto.Password); // HASH THE PASSWORD
                user.IsActive = false;

                // CHECK IF A USER WITH THE SAME EMAIL OR USERNAME ALREADY EXISTS
                var existingUserByEmail = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
                var existingUserByUsername = await _usersCollection.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
                if (existingUserByEmail != null)
                {
                    return new ConflictObjectResult("The email address provided is already associated with an existing account.");
                }
                if (existingUserByUsername != null)
                {
                    return new ConflictObjectResult("The username is already taken. Please use deferent username.");
                }

                // GENERATE ACTIVATION TOKEN AND SET EXPIRY (10 MINUTES)
                user.ActivationToken.Token = _commonService.GenerateActivationToken();
                user.ActivationToken.Expiry = DateTime.UtcNow.AddMinutes(10);

                // SEND ACTIVATION EMAIL
                var emailStatus =  await _mailService.SendActivationEmailAsync(user.Email, user.Username, user.ActivationToken.Token);

                // INSERT THE NEW USER INTO THE COLLECTION
                if (emailStatus)
                {
                    await _usersCollection.InsertOneAsync(user);
                    return new OkObjectResult("User registered successfully! Please check your email to activate your account.");
                }
                else
                {
                    return new BadRequestObjectResult("User registration failed because we were unable to send the activation email.Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ObjectResult("An error occurred while saving the user. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 2. METHOD TO LOGIN A USER
        public async Task<ActionResult<UserAuthToken>> LoginAsync(UserLogin loginDto)
        {
            try
            {
                // VALIDATE THE DEVICE ID FORMAT
                if (!_commonService.IsValidMacAddress(loginDto.DeviceId))
                {
                    return new BadRequestObjectResult("Invalid device ID format.");
                }

                // FIND THE USER BY USERNAME OR EMAIL
                var user = await _usersCollection.Find(u => u.Username == loginDto.Username || u.Email == loginDto.Username).FirstOrDefaultAsync();
            
                if (user == null || !_commonService.IsVerifyPassword(user.Password, loginDto.Password))
                {
                    return new BadRequestObjectResult("Invalid username or password.");
                }

                if (!user.IsActive)
                {
                    // GENERATE ACTIVATION TOKEN AND SET EXPIRY (10 MINUTES)
                    user.ActivationToken.Token = _commonService.GenerateActivationToken();
                    user.ActivationToken.Expiry = DateTime.UtcNow.AddMinutes(10);

                    var emailStatus = await _mailService.SendActivationEmailAsync(user.Email, user.Username, user.ActivationToken.Token);
                    if (emailStatus)
                    {
                        await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
                        return new BadRequestObjectResult("Your account is not activated. Please activate your account before logging in.");
                    }
                    else
                    {
                        return new BadRequestObjectResult("Your account is not activated, and we were unable to send the activation email.Please try again.");
                    }                
                }

                // GENERATE JWT AND REFRESH TOKEN
                var token = _tokenService.GenerateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                // CHECK IF DEVICE ID ALREADY EXISTS
                var existingAuthToken = user.AuthTokens.FirstOrDefault(t => t.DeviceId == loginDto.DeviceId);
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
                        DeviceId = loginDto.DeviceId,
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ObjectResult("An error occurred while login the user. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 3. METHOD TO ACTIVATE A USER ACCOUNT
        public async Task<IActionResult> ActivateAsync(string token)
        {
            try
            {
                // FIND THE USER BY ACTIVATION TOKEN
                var user = await _usersCollection.Find(u => u.ActivationToken.Token == token).FirstOrDefaultAsync();

                // CHECK IF USER EXISTS AND IF TOKEN IS VALID (NOT EXPIRED)
                if (user == null || user.ActivationToken.Expiry < DateTime.UtcNow)
                {
                    return new ContentResult
                    {
                        Content = await File.ReadAllTextAsync("Templates/ActivationFailed.html"),
                        ContentType = "text/html",
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // UPDATE USER AS ACTIVE AND CLEAR ACTIVATION TOKEN
                var update = Builders<User>.Update
                    .Set(u => u.IsActive, true)
                    .Unset(u => u.ActivationToken);

                // UPDATE USER DOCUMENT IN THE DATABASE
                await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);

                return new ContentResult
                {
                    Content = await File.ReadAllTextAsync("Templates/ActivationSuccess.html"),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ContentResult
                {
                    Content = await File.ReadAllTextAsync("Templates/ActivationFailed.html"),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        // 4. METHOD TO GENERATE AUTH TOKENS USING REFRESH TOKEN
        public async Task<ActionResult<UserAuthToken>> RefreshTokenAsync(UserRefreshToken request)
        {
            try
            {
                // VALIDATE THE DEVICE ID FORMAT
                if (!_commonService.IsValidMacAddress(request.DeviceId))
                {
                    return new BadRequestObjectResult("Invalid device ID format.");
                }

                // FIND THE USER BY THE REFRESH TOKEN
                var user = await _usersCollection.Find(u => u.AuthTokens.Any(t => t.RefreshToken == request.RefreshToken && t.DeviceId == request.DeviceId)).FirstOrDefaultAsync();
                if (user == null)
                {
                    return new BadRequestObjectResult("Invalid refresh token request.");
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

                // UPDATE THE AUTH TOKEN IN THE DATABASE
                await _usersCollection.UpdateOneAsync(
                    u => u.Id == user.Id &&
                         u.AuthTokens.Any(t => t.RefreshToken == request.RefreshToken && t.DeviceId == request.DeviceId),
                    Builders<User>.Update
                        .Set("AuthTokens.$.AccessToken", newToken)
                        .Set("AuthTokens.$.RefreshToken", newRefreshToken)
                        .Set("AuthTokens.$.Expiry", DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes))
                        .Set("AuthTokens.$.RefreshTokenExpiry", DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays))
                        .Set("AuthTokens.$.LastUsed", DateTime.UtcNow)
                );

                // RETURN THE NEW TOKENS
                var authTokenResponse = new UserAuthToken
                {
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                };

                return new OkObjectResult(authTokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ObjectResult("An error occurred while refreshing the token. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // 5. METHOD TO LOGOUT A USER
        public async Task<IActionResult> LogoutAsync(string deviceId)
        {
            try
            {
                // VALIDATE THE DEVICE ID FORMAT
                if (!_commonService.IsValidMacAddress(deviceId))
                {
                    return new BadRequestObjectResult("Invalid device ID format.");
                }

                // GET THE EMAIL FROM AUTHENTICATION HEADER
                var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

                // CHECK IF EMAIL IS NULL
                if (string.IsNullOrEmpty(email))
                {
                    return new NotFoundObjectResult(new { Status = "Error", Message = "User not found. Please ensure you are logged in." });
                }

                // FIND THE USER BY EMAIL AND DEVICE ID
                var user = await _usersCollection
                    .Find(u => u.Email == email && u.AuthTokens.Any(t => t.DeviceId == deviceId))
                    .FirstOrDefaultAsync();
               
                // CHECK IF USER EXISTS
                if (user == null)
                {
                    return new BadRequestObjectResult("User not found. Please ensure you are logged in.");
                }

                // REMOVE THE AUTH TOKEN FOR THE SPECIFIED DEVICE ID
                var update = Builders<User>.Update.PullFilter(u => u.AuthTokens, t => t.DeviceId == deviceId);

                // UPDATE USER DOCUMENT IN THE DATABASE
                await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
                return new OkObjectResult("User logged out successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ObjectResult("An error occurred while logging out. Please try again later.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<IActionResult> SendResetPasswordOtpAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> ValidateOtpAsync(UserValidateOtp validateOtpDto)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> ResetPasswordAsync(UserResetPassword resetPasswordDto)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> ChangeEmailAsync(UserChangeEmail changeEmailDto)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> ChangePasswordAsync(UserChangePassword changePasswordDto)
        {
            throw new NotImplementedException();
        }
    }
}
