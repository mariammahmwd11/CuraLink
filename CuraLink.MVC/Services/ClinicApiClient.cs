using CuraLink.MVC.Models.Clinics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

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
                "/api/doctor/clinics");

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
        /// NEW. Fetches the clinics owned/managed by the current doctor for the
        /// Dashboard / My Clinics section.
        ///
        /// ASSUMPTION (not yet confirmed against the API): this mirrors the existing
        /// POST /api/doctor/clinics create endpoint with a matching GET on the same
        /// route. If the real endpoint differs, only the URL below needs updating —
        /// nothing else in the MVC layer depends on it.
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
