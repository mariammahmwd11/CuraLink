using CuraLink.MVC.Models.Patients;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CuraLink.MVC.Services;

public class AppointmentApiClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppointmentApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // ============================================================
    // AVAILABLE SLOTS
    // ============================================================

    public async Task<List<AppointmentSlotViewModel>> GetAvailableSlotsAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"/api/doctors/{doctorId}/available-slots" +
            $"?date={date:yyyy-MM-dd}";

        using var request =
            CreateRequest(HttpMethod.Get, url);

        var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var slots =
                await response.Content
                    .ReadFromJsonAsync<List<SlotApiDto>>(
                        JsonOptions,
                        cancellationToken);

            return slots?
                .Select(s => new AppointmentSlotViewModel
                {
                    StartTime =
                        TimeSpan.Parse(s.StartTime),

                    EndTime =
                        TimeSpan.Parse(s.EndTime)
                })
                .ToList()
                ?? [];
        }

        var errorMessage =
            await response.Content
                .ReadAsStringAsync(cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to view available slots.");
        }

        throw new HttpRequestException(
            $"Failed to get available slots. " +
            $"Status: {response.StatusCode}. " +
            $"Error: {errorMessage}");
    }

    // ============================================================
    // BOOK APPOINTMENT
    // ============================================================

    public async Task<int> BookAppointmentAsync(
        Guid doctorId,
        Guid clinicId,
        DateOnly date,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            doctorId,
            clinicId,
            date = date.ToString("yyyy-MM-dd"),
            startTime = startTime.ToString(@"hh\:mm\:ss"),
            endTime = endTime.ToString(@"hh\:mm\:ss")
        };

        using var request =
            CreateRequest(
                HttpMethod.Post,
                "/api/appointments",
                payload);

        var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result =
                await response.Content
                    .ReadFromJsonAsync<BookAppointmentApiResponse>(
                        JsonOptions,
                        cancellationToken);

            return result?.AppointmentId
                ?? throw new Exception(
                    "Appointment ID was not returned.");
        }

        var errorMessage =
            await response.Content
                .ReadAsStringAsync(cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to book this appointment.");
        }

        if (response.StatusCode ==
                HttpStatusCode.Conflict ||
            response.StatusCode ==
                HttpStatusCode.BadRequest)
        {
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "The selected slot is no longer available. Please choose another slot."
                    : errorMessage);
        }

        throw new HttpRequestException(
            $"Failed to book appointment. " +
            $"Status: {response.StatusCode}. " +
            $"Error: {errorMessage}");
    }

    // ============================================================
    // CREATE STRIPE CHECKOUT SESSION
    // ============================================================

    public async Task<string> CreateCheckoutSessionAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        // IMPORTANT:
        // API endpoint is:
        // POST /api/payments/{appointmentId}/checkout

        var url =
            $"/api/payments/{appointmentId}/checkout";

        using var request =
            CreateRequest(
                HttpMethod.Post,
                url);

        var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to start this payment.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body =
                await response.Content
                    .ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Could not start the payment session. " +
                $"Status: {response.StatusCode}. " +
                $"{body}");
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<CheckoutSessionApiResponse>(
                    JsonOptions,
                    cancellationToken);

        if (result is null ||
            string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            throw new HttpRequestException(
                "The payment session did not return a checkout URL.");
        }

        return result.CheckoutUrl;
    }

    // ============================================================
    // GET APPOINTMENT
    // ============================================================

    public async Task<AppointmentDetailsViewModel?>
        GetMyAppointmentAsync(
            int appointmentId,
            CancellationToken cancellationToken = default)
    {
        using var request =
            CreateRequest(
                HttpMethod.Get,
                $"/api/appointments/{appointmentId}");

        var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to view this appointment.");
        }

        if (response.StatusCode is
            HttpStatusCode.NotFound or
            HttpStatusCode.Forbidden)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AppointmentDetailsViewModel>(
                JsonOptions,
                cancellationToken);
    }

    // ============================================================
    // CANCEL PENDING APPOINTMENT
    // ============================================================

    public async Task CancelPendingAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            CreateRequest(
                HttpMethod.Post,
                $"/api/appointments/{appointmentId}/cancel-pending");

        var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to cancel this appointment.");
        }
    }
    public async Task<List<MyAppointmentViewModel>> GetMyAppointmentsAsync(
    CancellationToken cancellationToken = default)
{
    using var request = CreateRequest(HttpMethod.Get, "/api/appointments/my");

    var response = await _httpClient.SendAsync(request, cancellationToken);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
        throw new UnauthorizedAccessException("You are not authorized to view your appointments.");

    response.EnsureSuccessStatusCode();

    return await response.Content
        .ReadFromJsonAsync<List<MyAppointmentViewModel>>(JsonOptions, cancellationToken)
        ?? [];
}
    // ============================================================
    // REQUEST HELPER
    // ============================================================

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        object? body = null)
    {
        var request =
            new HttpRequestMessage(
                method,
                url);

        var token = GetToken();

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        if (body is not null)
        {
            request.Content =
                JsonContent.Create(body);
        }

        return request;
    }

    // ============================================================
    // TOKEN
    // ============================================================

    private string GetToken()
    {
        var token =
            _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst("AccessToken")
                ?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Authentication token not found.");
        }

        return token;
    }

    // ============================================================
    // DTOs
    // ============================================================

    private class SlotApiDto
    {
        public string StartTime { get; set; } =
            string.Empty;

        public string EndTime { get; set; } =
            string.Empty;
    }

    private class BookAppointmentApiResponse
    {
        public int AppointmentId { get; set; }
    }

    private class CheckoutSessionApiResponse
    {
        public string? CheckoutUrl { get; set; }
    }
}