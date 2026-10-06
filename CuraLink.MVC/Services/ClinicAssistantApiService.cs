using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CuraLink.MVC.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; init; }
        public int StatusCode { get; init; }   // 0 = network failure
        public T? Data { get; init; }
        public string? RawMessage { get; init; }
    }

    public class InvitationPreviewDto
    {
        public string Email { get; set; } = "";
        public bool IsRegistered { get; set; }
        public string ClinicName { get; set; } = "";
    }
    public class ClinicAssistantDashboardDto
    {
        public string ClinicName { get; set; } = "";

        public int TotalToday { get; set; }

        public int Waiting { get; set; }

        public int CheckedIn { get; set; }

        public int Completed { get; set; }

        public List<TodayAppointmentDto> TodayAppointments { get; set; }
            = new();
    }

    public class TodayAppointmentDto
    {
        public int AppointmentId { get; set; }

        public string Time { get; set; } = "";

        public string PatientName { get; set; } = "";

        public string DoctorName { get; set; } = "";

        public string Status { get; set; } = "";
    }
    public class MessageDto
    {
        public string? Message { get; set; }
    }

    // Thin API client for the clinic-assistant invitation endpoints.
    // Same pattern as ChatController: IHttpClientFactory + ApiSettings:BaseUrl.
    public class ClinicAssistantApiService
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClinicAssistantApiService> _logger;

        public ClinicAssistantApiService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ClinicAssistantApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private string BaseUrl =>
            (_configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7188").TrimEnd('/');
        public Task<ApiResult<ClinicAssistantDashboardDto>> GetDashboardAsync(
    string accessToken) =>
    SendAsync<ClinicAssistantDashboardDto>(
        HttpMethod.Get,
        "/api/clinic-assistants/dashboard",
        null,
        accessToken);
        public Task<ApiResult<InvitationPreviewDto>> PreviewAsync(string token) =>
      SendAsync<InvitationPreviewDto>(
          HttpMethod.Get,
          $"/api/clinic-assistants/invitations?token={Uri.EscapeDataString(token)}",
          null, null);

        public Task<ApiResult<MessageDto>> RegisterAsync(
            string token, string firstName, string lastName, string phone, string password) =>
            SendAsync<MessageDto>(
                HttpMethod.Post,
                "/api/clinic-assistants/invitations/register",
                new { token, firstName, lastName, phone, password },
                null);

        public Task<ApiResult<MessageDto>> AcceptAsync(string token, string accessToken) =>
            SendAsync<MessageDto>(
                HttpMethod.Post,
                "/api/clinic-assistants/invitations/accept",
                new { token },
                accessToken);

        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body, string? bearer)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(method, BaseUrl + path);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(bearer))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
                if (body != null)
                    request.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request);
                var text = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    T? data = default;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try { data = JsonSerializer.Deserialize<T>(text, Json); }
                        catch (JsonException) { /* body not needed on success */ }
                    }
                    return new ApiResult<T> { Success = true, StatusCode = (int)response.StatusCode, Data = data };
                }

                _logger.LogWarning("Clinic assistant API {Method} {Path} returned {Status}: {Body}",
                    method, path.Split('?')[0], (int)response.StatusCode, text);

                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = (int)response.StatusCode,
                    RawMessage = ExtractMessage(text)
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Clinic assistant API request failed.");
                return new ApiResult<T> { Success = false, StatusCode = 0 };
            }
        }

        // Pulls a readable message out of {message}/{error}/{detail}/{title}/{errors:{...}}.
        private static string? ExtractMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

                if (doc.RootElement.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Object)
                {
                    var parts = new List<string>();
                    foreach (var prop in errors.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                            parts.AddRange(prop.Value.EnumerateArray()
                                .Where(e => e.ValueKind == JsonValueKind.String)
                                .Select(e => e.GetString()!));
                        else if (prop.Value.ValueKind == JsonValueKind.String)
                            parts.Add(prop.Value.GetString()!);
                    }
                    if (parts.Count > 0) return string.Join(" ", parts);
                }

                foreach (var name in new[] { "message", "error", "detail", "title" })
                    if (doc.RootElement.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String)
                        return v.GetString();
            }
            catch (JsonException) { }
            return null;
        }

        // Maps backend errors to user-friendly text. Never returns stack traces.
        public static string FriendlyMessage(int status, string? raw, bool allowValidationText = false)
        {
            var m = (raw ?? "").ToLowerInvariant();

            if (status == 0)
                return "We couldn't reach the server. Please check your connection and try again.";
            if (m.Contains("expired"))
                return "This invitation has expired. Please ask the clinic to send you a new one.";
            if (m.Contains("already been accepted") || m.Contains("already accepted") || m.Contains("accepted"))
                return "This invitation has already been accepted.";
            if (m.Contains("match") || m.Contains("another") || m.Contains("different"))
                return "This invitation was sent to a different email address. Please log in with the invited email.";
            if (m.Contains("already registered") || m.Contains("already exists") ||
                m.Contains("already taken") || m.Contains("already in use"))
                return "An account with this email already exists. Please log in instead.";
            if (status == 401 || status == 403)
                return "Please log in to accept this invitation.";
            if (status == 404 || m.Contains("invalid") || m.Contains("not found"))
                return "This invitation link is invalid.";
            if (allowValidationText && status == 400 && !string.IsNullOrWhiteSpace(raw) && raw.Length <= 300)
                return raw!; // validation text such as password rules
            if (status == 400)
                return "Please check the information you entered and try again.";
            return "Something went wrong. Please try again.";
        }
    }
}