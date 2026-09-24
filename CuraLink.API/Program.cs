using CuraLink.API.Endpoints.Admin;
using CuraLink.API.Endpoints.AuthEndPoints;
using CuraLink.API.Endpoints.Clinics;
using CuraLink.API.Endpoints.DoctorEndpoints;
using CuraLink.API.Endpoints.Doctors;
using CuraLink.API.Endpoints.Notifications;
using CuraLink.API.Endpoints.PatientEndpoints;
using CuraLink.API.Endpoints.PatientEndPoints;
using CuraLink.API.Endpoints.Patients;
using CuraLink.API.Endpoints.Prescriptions;
using CuraLink.API.Endpoints.Profile;
using CuraLink.API.Exceptions;
using CuraLink.Application;
using CuraLink.Infrastructure;
using CuraLink.Infrastructure.Identity;
using CuraLink.Infrastructure.Services.Notifications;
using Hangfire;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices(
    builder.Configuration);
builder.Services.AddApplicationServices(
    );


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<WebPushSettings>(
    builder.Configuration.GetSection("WebPush"));

var app = builder.Build();
//hangfire dashboard
app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedRolesAsync(roleManager);
    await IdentitySeeder.SeedAdminAsync(userManager);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


//minimal API endpoints
//authentication endpoints
app.MapLoginEndPoint();
app.MapRegisterPatientEndpoint();
app.MapRegisterDoctorEndpoint();
//admin api endpoints
app.MapPendingDoctorsEndpoint();
app.MapGetDoctorDocumentEndpoint();
app.MapTestEmail();
app.MapDownloadDoctorDocumentEndpoint();
app.MapVerifyDoctorEndpoint();
//clinic api endpoints
app.MapCreateClinicEndpoint();
app.MapGetDoctorClinicsEndpoint();
//patient api endpoints
app.MapUploadMedicalDocumentEndpoint();
app.MapGetPatientMedicalDocsEndpoint();
app.MapSearchDoctorsEndpoint();
//prescription api endpoints
app.MapCreatePrescriptionEndpoint();
app.MapGetPrescriptionPdfEndpoint();
app.MapGetDoctorPrescriptionsEndpoint();
//doctor api endpoints
app.MapGetMyPatientsEndpoint();
app.MapUpdateDoctorScheduleEndpoint();
app.MapGetDoctorScheduleEndpoint();
//notification api endpoints
app.MapRegisterNotificationSubscriptionEndpoint();
app.MapMarkNotificationAsReadEndpoint();
app.MapMarkAllNotificationsAsReadEndpoint();
app.MapGetUnreadNotificationCountEndpoint();
app.MapGetNotificationSubscriptionStatusEndpoint();
app.MapGetMyNotificationsEndpoint();
//profile api endpoints
app.MapUpdateProfileEndpoint();
app.MapGetProfileEndpoint();

app.Run();
