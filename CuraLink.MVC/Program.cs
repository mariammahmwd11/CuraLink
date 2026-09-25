using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7188/";

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<AdminApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ClinicApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<PatientApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); 
});
builder.Services.AddHttpClient<PrescriptionApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); 
});
builder.Services.AddHttpClient<NotificationApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<DoctorScheduleApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<AppointmentApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CuraLink.Auth";
        options.Cookie.HttpOnly = true;
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnet-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
