using CuraLink.MVC.Models.Doctors;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CuraLink.MVC.Services
{
    public class DoctorScheduleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DoctorScheduleApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<DoctorScheduleViewModel> GetScheduleAsync(
            CancellationToken cancellationToken = default)
        {
            var token = GetToken();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/doctor/schedule");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            var model = new DoctorScheduleViewModel();

            if (response.IsSuccessStatusCode)
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var apiResult = await response.Content
                    .ReadFromJsonAsync<List<ScheduleAvailabilityDto>>(jsonOptions, cancellationToken);

                return MergeApiResultIntoModel(model, apiResult);
            }

            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this schedule.");
            }

            throw new HttpRequestException(
                $"Failed to get schedule. Status: {response.StatusCode}. Error: {errorMessage}");
        }


        public async Task UpdateScheduleAsync(
            List<DayScheduleViewModel> days,
            CancellationToken cancellationToken = default)
        {
            var token = GetToken();

            var payload = new
            {
                availability = days
                    .Where(d => d.IsEnabled && d.StartTime.HasValue && d.EndTime.HasValue)
                    .Select(d => new
                    {
                        dayOfWeek = (int)d.DayOfWeek,
                        startTime = FormatTime(d.StartTime!.Value),
                        endTime = FormatTime(d.EndTime!.Value),
                        slotDurationMinutes = d.SlotDurationMinutes
                    })
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/api/doctor/schedule")
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this schedule.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(errorMessage) ? "Invalid schedule data." : errorMessage);
            }

            throw new HttpRequestException(
                $"Failed to update schedule. Status: {response.StatusCode}. Error: {errorMessage}");
        }

        private static string FormatTime(TimeSpan time) => time.ToString(@"hh\:mm\:ss");

        private static DoctorScheduleViewModel MergeApiResultIntoModel(
      DoctorScheduleViewModel model,
      List<ScheduleAvailabilityDto>? apiResult)
        {
            if (apiResult is null)
            {
                return model;
            }

            foreach (var item in apiResult)
            {
                var day = model.Days.FirstOrDefault(d => (int)d.DayOfWeek == item.DayOfWeek);

                if (day is null)
                {
                    continue;
                }

                day.IsEnabled = true;
                day.StartTime = TimeSpan.TryParse(item.StartTime, out var start) ? start : day.StartTime;
                day.EndTime = TimeSpan.TryParse(item.EndTime, out var end) ? end : day.EndTime;
                day.SlotDurationMinutes = item.SlotDurationMinutes > 0
                    ? item.SlotDurationMinutes
                    : day.SlotDurationMinutes;
            }

            return model;
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

      

        private class ScheduleAvailabilityDto
        {
            public Guid Id { get; set; }
            public int DayOfWeek { get; set; }
            public string StartTime { get; set; } = string.Empty;
            public string EndTime { get; set; } = string.Empty;
            public int SlotDurationMinutes { get; set; }
        }
    }
}