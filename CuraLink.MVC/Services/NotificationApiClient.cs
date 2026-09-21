using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CuraLink.MVC.Services;

public class NotificationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> RegisterSubscriptionAsync(
        string endpoint,
        string p256DH,
        string auth)
    {
        if (!TrySetAuthHeader())
        {
            return false;
        }

        var request = new
        {
            endpoint,
            p256DH,
            auth
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            "/api/notifications/subscription",
            content);

        return response.IsSuccessStatusCode;
    }

    // =========================================================
    // Subscription status — drives which dashboard card is shown
    // =========================================================
    public async Task<bool> GetSubscriptionStatusAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TrySetAuthHeader())
        {
            return false;
        }

        var response = await _httpClient.GetAsync(
            "/api/notifications/subscription-status",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<SubscriptionStatusResponse>(json, JsonOptions);

        return result?.Subscribed ?? false;
    }

    // =========================================================
    // In-app notifications (bell dropdown)
    // =========================================================
    public async Task<List<NotificationDto>> GetNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TrySetAuthHeader())
        {
            return new List<NotificationDto>();
        }

        var response = await _httpClient.GetAsync("/api/notifications", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new List<NotificationDto>();
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<List<NotificationDto>>(json, JsonOptions)
            ?? new List<NotificationDto>();
    }

    public async Task<int> GetUnreadCountAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TrySetAuthHeader())
        {
            return 0;
        }

        var response = await _httpClient.GetAsync(
            "/api/notifications/unread-count",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return 0;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<UnreadCountResponse>(json, JsonOptions);

        return result?.Count ?? 0;
    }

    public async Task<bool> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TrySetAuthHeader())
        {
            return false;
        }

        var response = await _httpClient.PatchAsync(
            $"/api/notifications/{id}/read",
            content: null,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkAllAsReadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TrySetAuthHeader())
        {
            return false;
        }

        var response = await _httpClient.PatchAsync(
            "/api/notifications/read-all",
            content: null,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    // =========================================================
    // Authentication — same convention as PatientApiClient
    // =========================================================
    private bool TrySetAuthHeader()
    {
        var accessToken = _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue("AccessToken");

        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return true;
    }

    // =========================================================
    // Response / DTO models
    // =========================================================
    private sealed class SubscriptionStatusResponse
    {
        public bool Subscribed { get; set; }
    }

    private sealed class UnreadCountResponse
    {
        public int Count { get; set; }
    }
}

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
