using CuraLink.MVC.Models.Clinics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CuraLink.MVC.Services
{
    public class ClinicApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClinicApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Guid> CreateClinicAsync(
            CreateClinicViewModel model)
        {
            var token = _httpContextAccessor.HttpContext?
                .User
                .FindFirst("AccessToken")
                ?.Value;

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException(
                    "Authentication token not found.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/doctor/CreateClinic");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            request.Content = JsonContent.Create(model);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result =
                    await response.Content
                        .ReadFromJsonAsync<CreateClinicResponse>();

                return result?.Id
                    ?? throw new Exception(
                        "Clinic ID was not returned.");
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to create a clinic.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Only doctors can create a clinic.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Invalid clinic data."
                        : errorMessage);
            }

            throw new HttpRequestException(
                $"Failed to create clinic. Status: {response.StatusCode}. " +
                $"Error: {errorMessage}");
        }

        /// <summary>
        /// Fetches the clinics owned/managed by the current doctor for the
        /// Dashboard / My Clinics section.
        ///
        /// Fails soft: on 404/network/deserialization issues it returns an empty
        /// list instead of throwing, so the Dashboard still renders its
        /// "No clinics yet" empty state instead of crashing the page.
        /// </summary>
        public async Task<List<ClinicSummaryViewModel>> GetMyClinicsAsync()
        {
            var token = _httpContextAccessor.HttpContext?
                .User
                .FindFirst("AccessToken")
                ?.Value;

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException(
                    "Authentication token not found.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/doctor/clinics");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                // Network-level failure — degrade to empty state rather than
                // taking the whole dashboard down.
                return new List<ClinicSummaryViewModel>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view clinics.");
            }

            if (!response.IsSuccessStatusCode)
            {
                // Includes 404 — endpoint may not exist yet on the API side.
                return new List<ClinicSummaryViewModel>();
            }

            try
            {
                // Matches CuraLink.Application ClinicDto exactly (Name, not
                // ClinicName; no Status/StaffCount — the API doesn't return
                // those yet). Mapped below into ClinicSummaryViewModel so the
                // Razor views can keep using their existing property names.
                var apiResult =
                    await response.Content
                        .ReadFromJsonAsync<List<ClinicApiDto>>();

                if (apiResult is null)
                {
                    return new List<ClinicSummaryViewModel>();
                }

                return apiResult.Select(dto => new ClinicSummaryViewModel
                {
                    Id = dto.Id,
                    ClinicName = dto.Name,
                    Address = dto.Address,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty,
                    ConsultationPrice = dto.ConsultationPrice
                    // Status stays at its default ("Active") and StaffCount
                    // stays null — the API doesn't return either field yet.
                }).ToList();
            }
            catch (System.Text.Json.JsonException)
            {
                // Response shape didn't match what we expect — degrade safely
                // instead of throwing an unhandled exception on the dashboard.
                return new List<ClinicSummaryViewModel>();
            }
        }

        // =========================================================
        // Edit / Delete
        // =========================================================

        /// <summary>
        /// One clinic of the current doctor, for pre-filling the Edit form.
        /// Reuses GET /api/doctor/clinics, so it only ever returns clinics the
        /// doctor owns (no extra "get by id" endpoint needed).
        /// </summary>
        public async Task<ClinicSummaryViewModel?> GetClinicAsync(Guid id)
        {
            var clinics = await GetMyClinicsAsync();
            return clinics.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// PUT /api/doctor/clinics/{id}
        /// Body: same shape as the create request (clinicName, address,
        /// consultationPrice, phoneNumber).
        /// </summary>
        public async Task UpdateClinicAsync(Guid id, CreateClinicViewModel model)
        {
            using var request = BuildAuthorizedRequest(HttpMethod.Put, $"/api/doctor/clinics/{id}");
            request.Content = JsonContent.Create(model);

            using var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response, "update the clinic");
        }

        /// <summary>
        /// DELETE /api/doctor/clinics/{id}
        /// </summary>
        public async Task DeleteClinicAsync(Guid id)
        {
            using var request = BuildAuthorizedRequest(HttpMethod.Delete, $"/api/doctor/clinics/{id}");

            using var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response, "delete the clinic");
        }

        private HttpRequestMessage BuildAuthorizedRequest(HttpMethod method, string url)
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

        // Maps API failures to exceptions the controller already knows how to show.
        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string action)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var message = await ReadErrorMessageAsync(response);

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException("You are not authorized.");

                case HttpStatusCode.Forbidden:
                    throw new InvalidOperationException(
                        message ?? $"You don't have permission to {action}.");

                case HttpStatusCode.NotFound:
                    throw new KeyNotFoundException(message ?? "Clinic not found.");

                case HttpStatusCode.Conflict:
                    throw new InvalidOperationException(
                        message ?? $"We couldn't {action} because of a conflict.");

                case HttpStatusCode.BadRequest:
                    throw new HttpRequestException(message ?? "Invalid clinic data.");

                case HttpStatusCode.MethodNotAllowed:
                    throw new HttpRequestException("This action is not available yet.");

                default:
                    throw new HttpRequestException($"We couldn't {action}. Please try again.");
            }
        }

        // Pulls a readable message from {message}/{detail}/{error}/{title}/{errors:{..}} or short plain text.
        private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
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
                        }
                        if (parts.Count > 0) return string.Join(" ", parts);
                    }

                    foreach (var name in new[] { "message", "detail", "error", "title" })
                    {
                        if (doc.RootElement.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String)
                            return v.GetString();
                    }
                }
                return null;
            }
            catch (JsonException)
            {
                // Plain text: only show it if it is short and not markup.
                var trimmed = text.Trim().Trim('"');
                return trimmed.Length <= 200 && !trimmed.StartsWith('<') ? trimmed : null;
            }
        }

        // Mirrors CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics.ClinicDto
        // exactly. Kept private/internal to this client so the MVC layer's
        // own ClinicSummaryViewModel can use different, more descriptive
        // names without the two shapes needing to match.
        private class ClinicApiDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public decimal ConsultationPrice { get; set; }
            public string? PhoneNumber { get; set; }
        }

        private class CreateClinicResponse
        {
            public Guid Id { get; set; }

            public string? Message { get; set; }
        }
    }
}
