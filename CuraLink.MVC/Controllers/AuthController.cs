using CuraLink.MVC.Models.Auth;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace CuraLink.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApiClient;

        public AuthController(AuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        // =========================
        // Login
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _authApiClient.LoginAsync(model);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage =
                        await GetApiErrorMessageAsync(response);

                    ModelState.AddModelError(
                        string.Empty,
                        errorMessage);

                    return View(model);
                }

                var json = await response.Content.ReadAsStringAsync();

                var loginResponse =
                    JsonSerializer.Deserialize<LoginResponseViewModel>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (loginResponse == null ||
                    string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Login failed. Please try again.");

                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        loginResponse.User.Id),

                    new Claim(
                        ClaimTypes.Name,
                        loginResponse.User.Email),

                    new Claim(
                        ClaimTypes.Email,
                        loginResponse.User.Email),

                    new Claim(
                        ClaimTypes.Role,
                        loginResponse.User.Role),

                    new Claim(
                        "AccessToken",
                        loginResponse.AccessToken),

                    new Claim(
                        "RefreshToken",
                        loginResponse.RefreshToken)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the server.");

                return View(model);
            }
        }


        // =========================
        // Register Patient
        // =========================

        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(
            RegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response =
                await _authApiClient.RegisterPatientAsync(model);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");

                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please try again.");

                return View(model);
            }

            return RedirectToAction(nameof(Login));
        }


        // =========================
        // Register Doctor
        // =========================

        [HttpGet]
        public IActionResult RegisterDoctor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDoctor(
            RegisterDoctorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response =
                await _authApiClient.RegisterDoctorAsync(model);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");

                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please try again.");

                return View(model);
            }

            return RedirectToAction(nameof(Login));
        }


        // =========================
        // API Error Message Helper
        // =========================

        private async Task<string> GetApiErrorMessageAsync(
            HttpResponseMessage response)
        {
            var error = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(error))
                return "An error occurred.";

            try
            {
                using var document =
                    JsonDocument.Parse(error);

                var root = document.RootElement;

                // ASP.NET Core ProblemDetails
                if (root.TryGetProperty(
                    "detail",
                    out var detail))
                {
                    return detail.GetString()
                           ?? "An error occurred.";
                }

                // In case API returns { message: "..." }
                if (root.TryGetProperty(
                    "message",
                    out var message))
                {
                    return message.GetString()
                           ?? "An error occurred.";
                }

                // In case API returns { error: "..." }
                if (root.TryGetProperty(
                    "error",
                    out var errorProperty))
                {
                    return errorProperty.GetString()
                           ?? "An error occurred.";
                }
            }
            catch (JsonException)
            {
                // Response is not JSON
            }

            // If API returned plain text
            return error;
        }
    }
}