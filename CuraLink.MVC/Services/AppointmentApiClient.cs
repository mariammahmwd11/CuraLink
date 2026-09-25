using CuraLink.MVC.Models.Patients;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CuraLink.MVC.Services
{
    public class AppointmentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<AppointmentSlotViewModel>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var token = GetToken();

            var url = $"/api/doctors/{doctorId}/available-slots?date={date:yyyy-MM-dd}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var slots = await response.Content
                    .ReadFromJsonAsync<List<SlotApiDto>>(jsonOptions, cancellationToken);

                return slots?
                    .Select(s => new AppointmentSlotViewModel
                    {
                        StartTime = TimeSpan.Parse(s.StartTime),
                        EndTime = TimeSpan.Parse(s.EndTime)
                    })
                    .ToList()
                    ?? [];
            }

            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You are not authorized to view available slots.");
            }

            throw new HttpRequestException(
                $"Failed to get available slots. Status: {response.StatusCode}. Error: {errorMessage}");
        }

        public async Task<int> BookAppointmentAsync(
            Guid doctorId,
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default)
        {
            var token = GetToken();

            var payload = new
            {
                doctorId,
                date = date.ToString("yyyy-MM-dd"),
                startTime = startTime.ToString(@"hh\:mm\:ss"),
                endTime = endTime.ToString(@"hh\:mm\:ss")
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/appointments")
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var result = await response.Content
                    .ReadFromJsonAsync<BookAppointmentApiResponse>(jsonOptions, cancellationToken);

                return result?.AppointmentId
                    ?? throw new Exception("Appointment ID was not returned.");
            }

            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You are not authorized to book this appointment.");
            }

            // Conflict/BadRequest: most likely the slot was just taken.
            if (response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "The selected slot is no longer available. Please choose another slot."
                        : errorMessage);
            }

            throw new HttpRequestException(
                $"Failed to book appointment. Status: {response.StatusCode}. Error: {errorMessage}");
        }

        private string GetToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Authentication token not found.");
            }

            return token;
        }

        private class SlotApiDto
        {
            public string StartTime { get; set; } = string.Empty;
            public string EndTime { get; set; } = string.Empty;
        }

        private class BookAppointmentApiResponse
        {
            public int AppointmentId { get; set; }
        }
    }
}

