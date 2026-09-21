using CuraLink.MVC.Models.Prescriptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CuraLink.MVC.Services
{
    public class PrescriptionApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>camelCase on the wire, plus "08:00:00" times.</summary>
        private static readonly JsonSerializerOptions SerializeOptions = BuildSerializeOptions();

        private static JsonSerializerOptions BuildSerializeOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new TimeSpanJsonConverter());
            return options;
        }

        public PrescriptionApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Same convention as PatientApiClient: the token lives in the
        /// "AccessToken" claim on the cookie principal and is attached per call.
        /// </summary>
        private string GetAccessToken()
        {
            var token = _httpContextAccessor.HttpContext?
                .User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No access token found on the current principal.");
            }

            return token!;
        }

        // =====================================================================
        // GET /api/doctors/patients
        // =====================================================================
        public async Task<List<DoctorPatientOptionViewModel>> GetMyPatientsAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "/api/doctors/patients");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "The API rejected the doctor's token.");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<DoctorPatientOptionViewModel>();
            }

            var dtos = JsonSerializer.Deserialize<List<DoctorPatientDto>>(json, JsonOptions)
                       ?? new List<DoctorPatientDto>();

            return dtos
                .Select(d => new DoctorPatientOptionViewModel
                {
                    PatientId = d.PatientId,
                    PatientUserId = d.PatientUserId,
                    FullName = string.IsNullOrWhiteSpace(d.FullName)
                        ? "Unnamed patient"
                        : d.FullName!,
                    Age = d.Age,
                    BloodType = d.BloodType
                })
                .OrderBy(p => p.FullName)
                .ToList();
        }

        // =====================================================================
        // POST /api/prescriptions
        // =====================================================================
        public async Task<PrescriptionCreateResult> CreatePrescriptionAsync(
            CreatePrescriptionViewModel model,
            CancellationToken cancellationToken = default)
        {
            var payload = new CreatePrescriptionRequest
            {
                PatientUserId = model.PatientUserId!.Value.ToString(),
                StartDate = model.StartDate!.Value.Date,
                EndDate = model.EndDate!.Value.Date,
                Items = model.Items
                    .Where(i => !i.IsEmpty())
                    .Select(i => new CreatePrescriptionItemRequest
                    {
                        MedicationName = i.MedicationName.Trim(),
                        Dosage = i.Dosage.Trim(),
                        Instructions = string.IsNullOrWhiteSpace(i.Instructions)
                            ? null
                            : i.Instructions!.Trim(),
                        DosageTimes = i.GetParsedDosageTimes()
                    })
                    .ToList()
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "/api/prescriptions")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload, SerializeOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "The API rejected the doctor's token.");
            }

            if (response.IsSuccessStatusCode)
            {
                return PrescriptionCreateResult.Success();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            return PrescriptionCreateResult.Failure(
                ExtractErrorMessage(body, response.StatusCode));
        }

        // =====================================================================
        // GET /api/prescriptions — doctor's own prescriptions list
        // =====================================================================
        public async Task<List<DoctorPrescriptionListItemViewModel>> GetMyPrescriptionsAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "/api/prescriptions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "The API rejected the doctor's token.");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<DoctorPrescriptionListItemViewModel>();
            }

            var dtos = JsonSerializer.Deserialize<List<DoctorPrescriptionDto>>(json, JsonOptions)
                       ?? new List<DoctorPrescriptionDto>();

            return dtos
                .Select(d => new DoctorPrescriptionListItemViewModel
                {
                    Id = d.Id,
                    PatientName = d.PatientName,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    CreatedAt = d.CreatedAt,
                    IsActive = d.IsActive,
                    Medications = d.Medications
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        // =====================================================================
        // GET /api/prescriptions/{id}/pdf — download, do NOT navigate to it
        // =====================================================================
        public async Task<(byte[] Bytes, string FileName)> DownloadPrescriptionPdfAsync(
            Guid prescriptionId,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"/api/prescriptions/{prescriptionId}/pdf");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "Your session has expired. Please log in again.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to export this prescription.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException(
                    "This prescription could not be found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"The prescription PDF could not be generated " +
                    $"(status {(int)response.StatusCode}). Please try again.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var fileName =
                response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? $"Prescription-{prescriptionId}.pdf";

            return (bytes, fileName.Trim('"'));
        }

        /// <summary>
        /// Best-effort read of a ProblemDetails / ValidationProblemDetails body so
        /// the real API message reaches the form instead of a generic failure.
        /// </summary>
        private static string ExtractErrorMessage(string body, HttpStatusCode statusCode)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("errors", out var errors)
                            && errors.ValueKind == JsonValueKind.Object)
                        {
                            var messages = new List<string>();

                            foreach (var field in errors.EnumerateObject())
                            {
                                if (field.Value.ValueKind == JsonValueKind.Array)
                                {
                                    messages.AddRange(
                                        field.Value.EnumerateArray()
                                            .Select(v => v.GetString() ?? string.Empty)
                                            .Where(s => !string.IsNullOrWhiteSpace(s)));
                                }
                            }

                            if (messages.Count > 0)
                            {
                                return string.Join(" ", messages);
                            }
                        }

                        foreach (var name in new[] { "detail", "title", "message", "error" })
                        {
                            if (root.TryGetProperty(name, out var value)
                                && value.ValueKind == JsonValueKind.String)
                            {
                                var text = value.GetString();

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    return text!;
                                }
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Not JSON — fall through to the generic message.
                }
            }

            return $"The prescription could not be saved ({(int)statusCode}). Please try again.";
        }

        // ---------------------------------------------------------------------
        // DTOs — private, mapped explicitly into ViewModels (ClinicDto lesson).
        // ---------------------------------------------------------------------

        private sealed class DoctorPatientDto
        {
            public Guid PatientId { get; set; }
            public Guid PatientUserId { get; set; }
            public string? FullName { get; set; }
            public int Age { get; set; }
            public string? BloodType { get; set; }
        }

        private sealed class DoctorPrescriptionDto
        {
            public Guid Id { get; set; }
            public string PatientName { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }
            public List<string> Medications { get; set; } = new();
        }

        // =====================================================================
        // POST /api/prescriptions body — matches CreatePrescriptionCommand.
        // UserId is NOT sent: the API reads the doctor from the "UserId" claim
        // on the bearer token attached above.
        // =====================================================================
        private sealed class CreatePrescriptionRequest
        {
            public string PatientUserId { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<CreatePrescriptionItemRequest> Items { get; set; } = new();
        }

        private sealed class CreatePrescriptionItemRequest
        {
            public string MedicationName { get; set; } = string.Empty;
            public string Dosage { get; set; } = string.Empty;
            public string? Instructions { get; set; }
            public List<TimeSpan> DosageTimes { get; set; } = new();
        }

        /// <summary>
        /// Writes TimeSpan as "HH:mm:ss" ("08:00:00"). Explicit rather than
        /// relying on the built-in converter, which is not available on every
        /// target framework.
        /// </summary>
        private sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(
                ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                TimeSpan.Parse(reader.GetString() ?? "00:00:00");

            public override void Write(
                Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
                writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
        }
    }

    public sealed class PrescriptionCreateResult
    {
        public bool IsSuccess { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static PrescriptionCreateResult Success() =>
            new() { IsSuccess = true };

        public static PrescriptionCreateResult Failure(string message) =>
            new() { IsSuccess = false, ErrorMessage = message };
    }
}
