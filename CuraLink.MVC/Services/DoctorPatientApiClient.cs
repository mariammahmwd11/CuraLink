using CuraLink.MVC.Models.Doctors;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CuraLink.MVC.Services;

public class DoctorPatientApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DoctorPatientApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<DoctorPatientViewModel>> GetMyPatientsAsync(
        CancellationToken cancellationToken = default)
    {
        var token = _httpContextAccessor.HttpContext?
            .User
            .FindFirst("AccessToken")
            ?.Value;

        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException(
                "Authentication token not found.");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/doctors/patients");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"Failed to load patients. " +
                $"Status: {response.StatusCode}. Error: {error}");
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<List<DoctorPatientApiModel>>(
                    cancellationToken)
            ?? new List<DoctorPatientApiModel>();

        return result
            .Select(x => new DoctorPatientViewModel
            {
                PatientId = x.PatientId,
                PatientUserId = x.PatientUserId,
                PatientName = x.FullName,
                Age = x.Age,
                BloodType = x.BloodType
            })
            .ToList();
    }
    

    public async Task<DoctorPatientProfileViewModel> GetPatientProfileAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"/api/doctors/patients/{patientId}");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        EnsureSuccessOrThrow(response);

        var profile = await response.Content
            .ReadFromJsonAsync<DoctorPatientProfileViewModel>(cancellationToken: cancellationToken);

        return profile ?? throw new HttpRequestException("The patient profile response was empty.");
    }

    // GET /api/doctors/patients/{patientId}/documents/{documentId}  (shown in the browser)
    public Task<DoctorMedicalDocumentFile> ViewMedicalDocumentAsync(
        Guid patientId,
        int documentId,
        CancellationToken cancellationToken = default) =>
        GetMedicalDocumentAsync(
            $"/api/doctors/patients/{patientId}/documents/{documentId}",
            cancellationToken);

    // GET /api/doctors/patients/{patientId}/documents/{documentId}/download
    public Task<DoctorMedicalDocumentFile> DownloadMedicalDocumentAsync(
        Guid patientId,
        int documentId,
        CancellationToken cancellationToken = default) =>
        GetMedicalDocumentAsync(
            $"/api/doctors/patients/{patientId}/documents/{documentId}/download",
            cancellationToken);

    private async Task<DoctorMedicalDocumentFile> GetMedicalDocumentAsync(
        string url,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, url);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        EnsureSuccessOrThrow(response);

        // Held in memory server-side (uploads are limited to 10 MB) and returned by the
        // MVC action, so the browser never sees the JWT or the API address.
        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var contentType = response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        var disposition = response.Content.Headers.ContentDisposition;
        var fileName = disposition?.FileNameStar ?? disposition?.FileName?.Trim('"');

        return new DoctorMedicalDocumentFile(content, contentType, fileName);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var token = _httpContextAccessor.HttpContext?
            .User
            .FindFirst("AccessToken")
            ?.Value;

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Authentication token not found.");
        }

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    // Same exception convention as the other typed clients:
    // 401 -> UnauthorizedAccessException, 404 -> KeyNotFoundException, anything else -> HttpRequestException.
    private static void EnsureSuccessOrThrow(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedAccessException("You are not authorized.");

            case HttpStatusCode.NotFound:
                throw new KeyNotFoundException("The requested item was not found.");

            default:
                // The raw response body is never surfaced to the user.
                throw new HttpRequestException($"The API returned status {(int)response.StatusCode}.");
        }
    }

    private class DoctorPatientApiModel
    {
        public Guid PatientId { get; set; }
        public Guid PatientUserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? BloodType { get; set; }
    }
}