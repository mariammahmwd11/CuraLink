using CloudinaryDotNet;
using CuraLink.API.Infrastructure.SignalR;
using CuraLink.Application.Common.Interfaces.AI;
using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.BackgroundJobs;
using CuraLink.Application.Common.Interfaces.Chat;
using CuraLink.Application.Common.Interfaces.Email;
using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Identity;
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Prescriptions;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Payments;
using CuraLink.Infrastructure.Authentication;
using CuraLink.Infrastructure.BackgroundJobs;
using CuraLink.Infrastructure.FileStorage;
using CuraLink.Infrastructure.Identity;
using CuraLink.Infrastructure.Presistance;
using CuraLink.Infrastructure.Presistance.Data;
using CuraLink.Infrastructure.Presistance.Repositories;
using CuraLink.Infrastructure.Services;
using CuraLink.Infrastructure.Services.AI;
using CuraLink.Infrastructure.Services.Email;
using CuraLink.Infrastructure.Services.Notifications;
using CuraLink.Infrastructure.Services.Prescriptions;
using CuraLink.Infrastructure.SignalR;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Configuration;
using System.Security.Claims;
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
            //hangfire configuration
            services.AddHangfire(config =>
            config.UseSqlServerStorage(
            configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfireServer();
            //questPDF
            QuestPDF.Settings.License =LicenseType.Community;


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
            services.AddScoped<IApplicationDbContext>(provider =>
             provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorDocumentRepository, DoctorDocumentRepository>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IClinicRepository , ClinicRepository>();
            services.AddScoped<IPatientRepository , PatientRepository>();
            services.AddScoped<IMedicalHistoryRepository , MedicalHistoryRepository>();
            services.AddScoped<IMedicalDocumentRepository , MedicalDocumentRepository>();
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
             services.AddHttpClient<IEmailService, BrevoEmailService>();
            services.AddScoped<IDoctorPatientRepository,DoctorPatientRepository>();
            services.AddScoped<INotificationSubscriptionRepository, NotificationSubscriptionRepository>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDosageReminderScheduler, DosageReminderScheduler>();
            services.AddScoped<IPrescriptionPdfService, PrescriptionPdfService>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserNameProvider, UserNameProvider>();
            services.AddScoped<IChatRealtimeService, ChatRealtimeService>();
            services.AddScoped<IDrugAssistantService, DrugAssistantService>();

            services.AddSignalR();

            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            services.AddScoped<
                IRealtimeNotificationService,
                RealtimeNotificationService>();
            services.Configure<JwtSettings>(
             configuration.GetSection("Jwt"));

         

            services.Configure<CloudinarySettings>(
                    configuration.GetSection("CloudinarySettings"));


            services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));

          

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

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireClaim(
                        ClaimTypes.Role,
                        "Admin");
                });
                options.AddPolicy("Patient", policy => policy.RequireRole("Patient"));
                options.AddPolicy("Doctor", policy => policy.RequireRole("Doctor"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("MvcClient", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7051")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.Configure<StripeSettings>(
    
                configuration.GetSection("Stripe"));
            return services;
        }
    }                   
}
