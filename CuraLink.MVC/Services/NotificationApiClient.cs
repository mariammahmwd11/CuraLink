using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CuraLink.MVC.Services;

public class NotificationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

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
        var accessToken = _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue("AccessToken");

        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

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
}