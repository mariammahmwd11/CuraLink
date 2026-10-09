using CuraLink.MVC.Models.Auth;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
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
        public IActionResult Login(string? invitationToken, string? returnUrl)
            => View(new LoginViewModel { InvitationToken = invitationToken, ReturnUrl = returnUrl });

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

                // A user can have several roles (e.g. Patient + Receptionist), so read them
                // all from the JWT. Falls back to the single Role returned by the API.
                var roles = ExtractRoles(loginResponse.AccessToken);
                if (roles.Count == 0 && !string.IsNullOrEmpty(loginResponse.User.Role))
                    roles.Add(loginResponse.User.Role);

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
                        "AccessToken",
                        loginResponse.AccessToken),

                    new Claim(
                        "RefreshToken",
                        loginResponse.RefreshToken)
                };

                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                // Sign in FIRST, then redirect.
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                // Clinic invitation: go back to the invitation page (now signed in -> Accept button).
                if (!string.IsNullOrEmpty(model.InvitationToken))
                    return RedirectToAction("Invitation", "ClinicAssistant", new { token = model.InvitationToken });

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl);
                // Clinic invitation...
                if (!string.IsNullOrEmpty(model.InvitationToken))
                {
                    return RedirectToAction(
                        "Invitation",
                        "ClinicAssistant",
                        new { token = model.InvitationToken });
                }

                if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                // Patient -> Patient Dashboard
                if (roles.Contains("Patient"))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Patient");
                }

                // Doctor -> Doctor Dashboard
                if (roles.Contains("Doctor"))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Doctor");
                }
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction(
                        "PendingDoctors",
                        "Admin");
                }

                // Clinic assistant -> Assistant Dashboard
                if (roles.Contains("Receptionist") &&
                    !roles.Contains("Doctor") &&
                    !roles.Contains("Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Assistant");
                }

                // Other roles -> Home for now
                return RedirectToAction(
                    "Index",
                    "Home");
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

        // Reads every role claim from the JWT payload (no signature validation needed here:
        // the API already issued the token and validates it on every call).
        private static List<string> ExtractRoles(string jwt)
        {
            var roles = new List<string>();
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return roles;

                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));

                foreach (var name in new[] { "role", "roles", ClaimTypes.Role })
                {
                    if (!doc.RootElement.TryGetProperty(name, out var value)) continue;

                    if (value.ValueKind == JsonValueKind.String)
                        roles.Add(value.GetString()!);
                    else if (value.ValueKind == JsonValueKind.Array)
                        roles.AddRange(value.EnumerateArray()
                            .Where(e => e.ValueKind == JsonValueKind.String)
                            .Select(e => e.GetString()!));
                }
            }
            catch (Exception)
            {
                // Malformed token: caller falls back to the Role returned by the API.
            }
            return roles.Distinct().ToList();
        }

        [HttpGet]
        public IActionResult GetAccessToken()
        {
            var accessToken = User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized();

            return Ok(new
            {
                accessToken
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }

    }
}