using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Infrastructure.Authentication;
using CuraLink.Infrastructure.FileStorage;
using CuraLink.Infrastructure.Identity;
using CuraLink.Infrastructure.Presistance.Data;
using CuraLink.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CuraLink.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(
               configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.Configure<JwtSettings>(
             configuration.GetSection("Jwt"));

            services.Configure<CloudinarySettings>(
                    configuration.GetSection("CloudinarySettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var jwtSettings = configuration
                        .GetSection("Jwt")
                        .Get<JwtSettings>()!;

            

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });
            services.AddAuthorization();
            return services;
        }
    }                   
}
