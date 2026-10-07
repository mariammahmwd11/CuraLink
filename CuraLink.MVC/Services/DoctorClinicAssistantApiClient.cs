using CuraLink.MVC.Models.Doctors;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CuraLink.MVC.Services;

public class DoctorClinicAssistantApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DoctorClinicAssistantApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ClinicAssistantViewModel>> GetAssistantsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get, $"/api/clinics/{clinicId}/assistants");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedAccessException();
            case HttpStatusCode.Forbidden:
            case HttpStatusCode.NotFound:
                throw new KeyNotFoundException("Clinic not found.");
        }

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"The API returned status {(int)response.StatusCode}.");

        return await response.Content
                   .ReadFromJsonAsync<List<ClinicAssistantViewModel>>(cancellationToken)
               ?? new List<ClinicAssistantViewModel>();
    }

    /// <returns>null on success, otherwise a friendly error message.</returns>
    public async Task<string?> InviteAsync(
        Guid clinicId,
        string email,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post, $"/api/clinics/{clinicId}/assistants/invite");

        request.Content = JsonContent.Create(new { email });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return null;

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => throw new UnauthorizedAccessException(),
            HttpStatusCode.BadRequest => "Please enter a valid email address.",
            HttpStatusCode.Forbidden => "You are not authorized to manage this clinic.",
            HttpStatusCode.NotFound => "Clinic not found.",
            HttpStatusCode.Conflict => "An active invitation already exists for this email.",
            _ => "Something went wrong while sending the invitation. Please try again."
        };
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Authentication token not found.");

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}