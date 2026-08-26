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
    }
}
