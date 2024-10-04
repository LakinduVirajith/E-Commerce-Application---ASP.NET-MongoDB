using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using E_Commerce_Application___ASP.NET_MongoDB.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

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
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
            builder.Services.AddScoped<MailService>();
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.AddSingleton<TokenService>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddScoped<CommonService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // 3. SWAGGER CONFIGURATION
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

            // 4. HTTP REQUEST PIPELINE CONFIGURATION
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
