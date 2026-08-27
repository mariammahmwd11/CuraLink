using CuraLink.MVC.Models.Auth;

namespace CuraLink.MVC.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginViewModel model)
        {
            return await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                model);
        }

        public async Task<HttpResponseMessage> RegisterPatientAsync(
            RegisterPatientViewModel model)
        {
            return await _httpClient.PostAsJsonAsync(
                "api/auth/register-patient",
                model);
        }

        public async Task<HttpResponseMessage> RegisterDoctorAsync(
            RegisterDoctorViewModel model)
        {
            using (var content = new MultipartFormDataContent())
            {
                // Add form fields
                content.Add(new StringContent(model.FirstName ?? ""), "FirstName");
                content.Add(new StringContent(model.LastName ?? ""), "LastName");
                content.Add(new StringContent(model.Email ?? ""), "Email");
                content.Add(new StringContent(model.Password ?? ""), "Password");
                content.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
                content.Add(new StringContent(model.Specialty ?? ""), "Specialty");
                content.Add(new StringContent(model.SyndicateId ?? ""), "SyndicateId");

                // Add file if present
                if (model.LicenseDocument != null)
                {
                    var fileContent = new StreamContent(model.LicenseDocument.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.LicenseDocument.ContentType ?? "application/octet-stream");
                    content.Add(fileContent, "LicenseDocument", model.LicenseDocument.FileName);
                }

                return await _httpClient.PostAsync(
                    "api/auth/register-doctor",
                    content);
            }
        }
    }
}
