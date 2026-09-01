using CuraLink.MVC.Models.Patients;
using System.Net;
using System.Net.Http.Headers;

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

            var errorMessage = await response.Content.ReadAsStringAsync();

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


        public async Task<List<MedicalDocumentViewModel>> GetMedicalDocumentsAsync()
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

        private string GetToken()
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

            return token;
        }

        private class UploadMedicalDocumentResponse
        {
            public int Id { get; set; }
            public string? Message { get; set; }
        }
    }
}