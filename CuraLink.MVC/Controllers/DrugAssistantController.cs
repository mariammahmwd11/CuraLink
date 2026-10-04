using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize]
    public class DrugAssistantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DrugAssistantController> _logger;

        public DrugAssistantController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<DrugAssistantController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // GET /DrugAssistant
        [HttpGet]
        public IActionResult Index() => View();

        public record QueryRequest(string? Query);

        // Thin same-origin proxy (token stays server-side), same pattern as ChatController.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            _logger.LogWarning("DrugAssistant.Query HIT - Query: {Query}", request?.Query);

            if (string.IsNullOrWhiteSpace(request?.Query) || request.Query.Length > 500)
            {
                _logger.LogWarning("DrugAssistant.Query rejected: empty or too long query.");
                return BadRequest();
            }

            var token = User.FindFirst("AccessToken")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("DrugAssistant proxy: no access token available for user.");
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7188").TrimEnd('/');

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var message = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/api/drug-assistant/query");
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Content = new StringContent(
                    JsonSerializer.Serialize(new { query = request.Query.Trim() }),
                    Encoding.UTF8,
                    "application/json");

                using var response = await client.SendAsync(message);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return StatusCode((int)HttpStatusCode.Unauthorized);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("DrugAssistant API returned 400: {Body}", errorBody);
                    return BadRequest();
                }
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("DrugAssistant API returned {Status}.", (int)response.StatusCode);
                    return StatusCode((int)HttpStatusCode.BadGateway);
                }

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
                _logger.LogError(ex, "DrugAssistant proxy request failed.");
                return StatusCode((int)HttpStatusCode.BadGateway);
            }
        }
    }
}