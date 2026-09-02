using CuraLink.MVC.Models.Patients;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CuraLink.MVC.Services
{
    public class PatientApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatientApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // =========================================================
        // Medical Documents
        // =========================================================

        public async Task<int> UploadMedicalDocumentAsync(IFormFile file)
        {
            var token = GetToken();

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "File", file.FileName);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/patient/medical-documents")
            {
                Content = content
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result =
                    await response.Content
                        .ReadFromJsonAsync<UploadMedicalDocumentResponse>();

                return result?.Id
                    ?? throw new Exception("Document ID was not returned.");
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to upload documents.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Invalid file."
                        : errorMessage);
            }

            throw new HttpRequestException(
                $"Failed to upload document. Status: {response.StatusCode}. " +
                $"Error: {errorMessage}");
        }

        public async Task<List<MedicalDocumentViewModel>>
            GetMedicalDocumentsAsync()
        {
            var token = GetToken();

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/patient/View-medical-records");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var documents =
                    await response.Content
                        .ReadFromJsonAsync<List<MedicalDocumentViewModel>>();

                return documents ?? [];
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view medical documents.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Patient not found."
                        : errorMessage);
            }

            throw new HttpRequestException(
                $"Failed to get medical documents. " +
                $"Status: {response.StatusCode}. " +
                $"Error: {errorMessage}");
        }


        // =========================================================
        // Doctors Search / Directory
        // =========================================================

        public async Task<DoctorSearchResult> SearchDoctorsAsync(
            string? doctorName,
            string? specialty,
            string? governorate,
            int pageNumber,
            int pageSize)
        {
            var token = GetToken();

            var query = new StringBuilder();

            query.Append(
                "/api/patient/doctors?pageNumber=")
                .Append(pageNumber);

            query.Append(
                "&pageSize=")
                .Append(pageSize);

            if (!string.IsNullOrWhiteSpace(doctorName))
            {
                query.Append("&doctorName=")
                    .Append(
                        Uri.EscapeDataString(
                            doctorName.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                query.Append("&specialty=")
                    .Append(
                        Uri.EscapeDataString(
                            specialty.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(governorate))
            {
                query.Append("&governorate=")
                    .Append(
                        Uri.EscapeDataString(
                            governorate.Trim()));
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                query.ToString());

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await _httpClient.SendAsync(request);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view doctors.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody =
                    await response.Content
                        .ReadAsStringAsync();

                throw new HttpRequestException(
                    $"Failed to search doctors. " +
                    $"Status: {response.StatusCode}. " +
                    $"Error: {errorBody}");
            }

            var jsonOptions =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            var apiResult =
                await response.Content
                    .ReadFromJsonAsync<SearchDoctorsApiResponse>(
                        jsonOptions);

            if (apiResult is null)
            {
                return new DoctorSearchResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            return new DoctorSearchResult
            {
                Items = apiResult.Items
                    .Select(dto => new DoctorListItemViewModel
                    {
                        Id = dto.Id,
                        FullName = dto.FullName,
                        Specialty = dto.Specialty,
                        Address = dto.Address,
                        Governorate = dto.Governorate,
                        ConsultationPrice =
                            dto.ConsultationPrice,
                        Rating = dto.Rating,
                        AvailableDates =
                            dto.AvailableDates
                    })
                    .ToList(),

                PageNumber = apiResult.PageNumber,

                PageSize = apiResult.PageSize,

                TotalCount = apiResult.TotalCount,

                TotalPages = apiResult.TotalPages
            };
        }


        // =========================================================
        // Authentication
        // =========================================================

        private string GetToken()
        {
            var token =
                _httpContextAccessor.HttpContext?
                    .User
                    .FindFirst("AccessToken")
                    ?.Value;

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException(
                    "Authentication token not found.");
            }

            return token;
        }


        // =========================================================
        // API Response Models
        // =========================================================

        private class UploadMedicalDocumentResponse
        {
            public int Id { get; set; }

            public string? Message { get; set; }
        }

        private class SearchDoctorsApiResponse
        {
            public List<DoctorApiDto> Items { get; set; }
                = new();

            public int PageNumber { get; set; }

            public int PageSize { get; set; }

            public int TotalCount { get; set; }

            public int TotalPages { get; set; }
        }

        private class DoctorApiDto
        {
            public Guid Id { get; set; }

            public string FullName { get; set; }
                = string.Empty;

            public string Specialty { get; set; }
                = string.Empty;

            public string Address { get; set; }
                = string.Empty;

            public string Governorate { get; set; }
                = string.Empty;

            public decimal ConsultationPrice { get; set; }

            public double Rating { get; set; }

            public List<DateTime> AvailableDates { get; set; }
                = new();
        }
    }


    // =============================================================
    // Doctors Search Result
    // =============================================================

    public class DoctorSearchResult
    {
        public List<DoctorListItemViewModel> Items { get; set; }
            = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}