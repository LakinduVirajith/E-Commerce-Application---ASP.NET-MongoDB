using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using E_Commerce_Application___ASP.NET_MongoDB.Interfaces;
using E_Commerce_Application___ASP.NET_MongoDB.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace E_Commerce_Application___ASP.NET_MongoDB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. ADD SERVICES TO THE CONTAINER
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // 2. DEPENDENCY INJECTION
            builder.Services.AddSingleton<IMongoDbService , MongoDbService>();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
            builder.Services.AddSingleton<IMailService, MailService>();
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddSingleton<ICommonService, CommonService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ADD THIS LINE TO REGISTER IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // 3. JWT AUTHENTICATION CONFIGURATION
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secret = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not set in configuration."));
            var expiryInMinutes = double.TryParse(jwtSettings["TokenExpiryInMinutes"], out var minuts) ? minuts : 120;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ClockSkew = TimeSpan.FromHours(expiryInMinutes)
                };
            });

            // 4. SWAGGER CONFIGURATION
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "E-Commerce Application Backend API",
                    Description = "API for managing and handling backend operations, including user authentication, user management, product management, cart management and more."
                });

                c.TagActionsBy(api => new List<string> { api.ActionDescriptor.RouteValues["controller"] + " Controllers" });

                // INCLUDE XML COMMENTS IF THEY EXIST
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // DEFINE THE SECURITY SCHEME FOR JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 6465asdasd6561asd...')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                // ADD SECURITY REQUIREMENT FOR THE ENDPOINTS
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // 5. HTTP REQUEST PIPELINE CONFIGURATION
            if (app.Environment.IsDevelopment())
            {
                // ENABLE SWAGGER IN DEVELOPMENT MODE
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                });
            }

            app.UseHttpsRedirection();

            // ENABLE AUTHENTICATION AND AUTHORIZATION MIDDLEWARE
            app.UseAuthentication();
            app.UseAuthorization();

            // MAP CONTROLLERS
            app.MapControllers();

            app.Run();
        }
    }
}
