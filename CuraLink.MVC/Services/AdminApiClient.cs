using CuraLink.MVC.Models.Admin;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CuraLink.MVC.Services
{
    /// <summary>
    /// Thin HttpClient wrapper around the existing backend Admin doctor
    /// verification APIs. Follows the same pattern as <see cref="AuthApiClient"/>:
    /// no business logic here, just building requests and returning the raw
    /// HttpResponseMessage for the controller to interpret.
    ///
    /// Every call attaches the signed-in admin's JWT (stored as a claim on
    /// the MVC cookie identity after login) as a Bearer token, so the
    /// backend's own "AdminOnly" authorization policy is still the real
    /// gatekeeper - this client never bypasses it.
    /// </summary>
    public class AdminApiClient
    {
        private const string AccessTokenClaimType = "AccessToken";

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<HttpResponseMessage> GetPendingDoctorsAsync(
            CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(HttpMethod.Get, "api/admin/pending-doctors");
            return _httpClient.SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> GetDoctorDocumentsAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(
                HttpMethod.Get,
                $"api/admin/doctors/{doctorId}/documents");

            return _httpClient.SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> VerifyDoctorAsync(
            Guid doctorId,
            VerifyDoctorRequestViewModel body,
            CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(
                HttpMethod.Put,
                $"api/admin/verify-doctor/{doctorId}");

            request.Content = JsonContent.Create(body);

            return _httpClient.SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> DownloadDoctorDocumentAsync(
            Guid doctorId,
            int documentId,
            CancellationToken cancellationToken = default)
        {
            var request = CreateRequest(
                HttpMethod.Get,
                $"api/admin/doctors/{doctorId}/documents/{documentId}/download");

            return _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl)
        {
            var request = new HttpRequestMessage(method, relativeUrl);

            var token = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(AccessTokenClaimType)?
                .Value;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }
    }
}
