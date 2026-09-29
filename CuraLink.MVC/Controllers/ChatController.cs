using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // GET /Chat/Index?appointmentId=1015
        [HttpGet]
        public IActionResult Index(int appointmentId)
        {
            if (appointmentId <= 0)
            {
                return BadRequest("A valid appointmentId is required.");
            }

            ViewBag.AppointmentId = appointmentId;
            ViewBag.CurrentUserId = ResolveUserId();
            return View();
        }

        // ---------- Thin same-origin proxies (token stays server-side) ----------

        [HttpGet]
        public Task<IActionResult> Messages(int appointmentId)
        {
            return ForwardAsync(HttpMethod.Get, $"/api/chat/{appointmentId}/messages", null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int appointmentId)
        {
            _logger.LogWarning(
                "🔥 ChatController.Send HIT - AppointmentId: {AppointmentId}",
                appointmentId);

            var body = await ReadBodyAsync();

            _logger.LogWarning(
                "🔥 Chat body: {Body}",
                body);

            return await ForwardAsync(
                HttpMethod.Post,
                $"/api/chat/{appointmentId}/messages",
                body);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> MarkRead(int appointmentId)
        {
            return ForwardAsync(HttpMethod.Post, $"/api/chat/{appointmentId}/read", "{}");
        }

        // ---------- Helpers ----------

        private string? ResolveUserId()
        {
            return User.FindFirstValue("UserId")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult(User.FindFirst("AccessToken")?.Value);
        }
        private async Task<string> ReadBodyAsync()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        private async Task<IActionResult> ForwardAsync(HttpMethod method, string path, string? jsonBody)
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Chat proxy: no access token available for user.");
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
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                using var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content,
                    ContentType = "application/json"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Chat proxy request to {Path} failed.", path);
                return StatusCode((int)HttpStatusCode.BadGateway);
            }
        }
    }
}