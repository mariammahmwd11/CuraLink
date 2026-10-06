using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    // Clinic assistant = a user with the Receptionist role linked to a clinic.
    // Pages are plain views; data comes from thin same-origin proxies to the API
    // (same pattern as ChatController: the JWT stays server-side).
    [Authorize(Roles = "Receptionist")]
    public class AssistantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AssistantController> _logger;

        public AssistantController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AssistantController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // ---------- Pages ----------

        [HttpGet] public IActionResult Index() => View();
        [HttpGet] public IActionResult Patients() => View();
        [HttpGet] public IActionResult CreatePatient() => View();
        [HttpGet] public IActionResult TodayAppointments() => View();

        [HttpGet]
        public IActionResult BookAppointment(string? patientId, string? patientName)
        {
            ViewBag.PatientId = patientId;
            ViewBag.PatientName = patientName;
            return View();
        }

        // ---------- Proxies (JSON) ----------

        [HttpGet]
        public Task<IActionResult> DashboardData() =>
            ForwardAsync(HttpMethod.Get, "/api/clinic-assistants/dashboard", null);

        [HttpGet]
        public Task<IActionResult> TodayData() =>
            ForwardAsync(HttpMethod.Get, "/api/clinic-assistants/appointments/today", null);

        [HttpGet]
        public Task<IActionResult> SearchPatients(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return Task.FromResult<IActionResult>(BadRequest(new { message = "Enter a name, email or phone number to search." }));

            return ForwardAsync(HttpMethod.Get,
                $"/api/clinic-assistants/patients/search?search={Uri.EscapeDataString(search.Trim())}", null);
        }

        public record CreatePatientRequest(
            string? FirstName, string? LastName, string? Email, string? Phone,
            string? DateOfBirth, string? BloodType, string? MedicalHistoryNotes);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreatePatientApi([FromBody] CreatePatientRequest? r)
        {
            if (r == null)
                return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid request." }));

            // Whitelist: only the fields the API expects.
            var body = JsonSerializer.Serialize(new
            {
                firstName = r.FirstName,
                lastName = r.LastName,
                email = r.Email,
                phone = r.Phone,
                dateOfBirth = r.DateOfBirth,
                bloodType = string.IsNullOrWhiteSpace(r.BloodType) ? null : r.BloodType,
                medicalHistoryNotes = string.IsNullOrWhiteSpace(r.MedicalHistoryNotes) ? null : r.MedicalHistoryNotes
            });
            return ForwardAsync(HttpMethod.Post, "/api/clinic-assistants/patients", body);
        }

        public record BookRequest(string? PatientId, string? Date, string? StartTime, string? EndTime);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> BookApi([FromBody] BookRequest? r)
        {
            if (r == null)
                return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid request." }));

            // Only these four fields. The API derives receptionist, clinic and doctor from the JWT.
            var body = JsonSerializer.Serialize(new
            {
                patientId = r.PatientId,
                date = r.Date,
                startTime = r.StartTime,
                endTime = r.EndTime
            });
            return ForwardAsync(HttpMethod.Post, "/api/clinic-assistants/appointments", body);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CheckIn(int id)
        {
            if (id <= 0)
                return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid appointment." }));

            return ForwardAsync(HttpMethod.Post, $"/api/clinic-assistants/appointments/{id}/check-in", null);
        }

        // ---------- Helper ----------

        private async Task<IActionResult> ForwardAsync(HttpMethod method, string path, string? jsonBody)
        {
            _logger.LogInformation(
    "Assistant proxy user: {User}, authenticated: {Authenticated}, token exists: {HasToken}",
    User.Identity?.Name,
    User.Identity?.IsAuthenticated,
    !string.IsNullOrEmpty(User.FindFirst("AccessToken")?.Value));
            var token = User.FindFirst("AccessToken")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Assistant proxy: no access token available for user.");
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7188").TrimEnd('/');

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(method, baseUrl + path);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (jsonBody != null)
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Assistant API {Method} {Path} returned {Status}: {Body}",
                        method, path.Split('?')[0], (int)response.StatusCode, content);

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content,
                    ContentType = "application/json"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Assistant proxy request to {Path} failed. Message: {Message}",
                    path,
                    ex.Message);

                return StatusCode(
                    (int)HttpStatusCode.BadGateway,
                    new
                    {
                        message = "Could not connect to the API.",
                        error = ex.Message
                    });
            }
        }
    }
}