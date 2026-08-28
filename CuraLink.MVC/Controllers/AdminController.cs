using CuraLink.MVC.Models.Admin;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace CuraLink.MVC.Controllers
{
    /// <summary>
    /// Admin-only doctor verification screens.
    ///
    /// Architecture, per feature request:
    ///   Browser (fetch/JS)
    ///     -> AdminController (this class)
    ///       -> AdminApiClient (HttpClient wrapper)
    ///         -> Backend API (/api/admin/...), protected by the existing
    ///            "AdminOnly" JWT policy.
    ///
    /// Authorization is enforced in two layers, as required:
    ///   1. [Authorize(Roles = "Admin")] below - a signed-out user or a
    ///      non-admin user is redirected/blocked before any view or data is
    ///      returned, regardless of what buttons the client happens to render.
    ///   2. The backend's own "AdminOnly" policy on every proxied call - the
    ///      admin's JWT (captured at login, see AuthController) is forwarded
    ///      as a Bearer token on every request, so the backend re-validates
    ///      the caller independently. Nothing about the backend's JWT setup
    ///      is touched here.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AdminApiClient _adminApiClient;

        public AdminController(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        [HttpGet]
        public IActionResult PendingDoctors()
        {
            return View("Index");
        }

        /// <summary>
        /// GET /Admin/GetPendingDoctorsData
        /// Proxies GET /api/admin/pending-doctors for the page's initial load.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPendingDoctorsData(
            CancellationToken cancellationToken)
        {
            var response = await _adminApiClient.GetPendingDoctorsAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return await ForwardErrorAsync(response, "Failed to load pending doctors.", cancellationToken);

            var doctors = await response.Content
                .ReadFromJsonAsync<List<PendingDoctorViewModel>>(cancellationToken: cancellationToken);

            return Json(doctors ?? new List<PendingDoctorViewModel>());
        }

        /// <summary>
        /// GET /Admin/GetDoctorDocumentsData?doctorId=...
        /// Proxies GET /api/admin/doctors/{doctorId}/documents for the review modal.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDoctorDocumentsData(
            Guid doctorId,
            CancellationToken cancellationToken)
        {
            var response = await _adminApiClient.GetDoctorDocumentsAsync(doctorId, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return await ForwardErrorAsync(response, "Failed to load verification documents.", cancellationToken);

            var documents = await response.Content
                .ReadFromJsonAsync<List<DoctorDocumentViewModel>>(cancellationToken: cancellationToken);

            return Json(documents ?? new List<DoctorDocumentViewModel>());
        }

        /// <summary>
        /// POST /Admin/VerifyDoctor?doctorId=...
        /// Proxies PUT /api/admin/verify-doctor/{doctorId} for both approve
        /// and reject actions (isApproved distinguishes the two).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDoctor(
            Guid doctorId,
            [FromBody] VerifyDoctorRequestViewModel request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                return BadRequest(new { message = "Invalid request." });

            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.RejectionReason))
                return BadRequest(new { message = "A rejection reason is required." });

            var response = await _adminApiClient.VerifyDoctorAsync(doctorId, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ForwardErrorAsync(
                    response,
                    request.IsApproved ? "Failed to approve doctor." : "Failed to reject doctor.",
                    cancellationToken);
            }

            return Json(new
            {
                success = true,
                message = request.IsApproved
                    ? "Doctor approved successfully."
                    : "Doctor rejected successfully."
            });
        }

        /// <summary>
        /// GET /Admin/DownloadDocument?doctorId=...&amp;documentId=...
        /// "View Document" opens this URL in a new tab. The browser sends the
        /// admin's MVC auth cookie automatically; this action then forwards
        /// the admin's JWT to the existing, protected
        /// GET /api/admin/doctors/{doctorId}/documents/{documentId}/download
        /// endpoint and streams the file back. The raw storage URL returned
        /// by GetDoctorDocumentsData is never used directly, since it is not
        /// publicly browsable.
        ///
        /// Served as "inline" (not "attachment") so the browser tab previews
        /// the file (PDF/image) instead of forcing a download to disk.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadDocument(
      Guid doctorId,
      int documentId,
      CancellationToken cancellationToken)
        {
            var response = await _adminApiClient.DownloadDoctorDocumentAsync(
                doctorId,
                documentId,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                return StatusCode(
                    (int)response.StatusCode,
                    errorBody);
            }

            var contentType =
                response.Content.Headers.ContentType?.ToString()
                ?? "application/octet-stream";

            var fileName =
                response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName
                ?? "document";

            fileName = fileName.Trim('"');

            // Copy API response into a stream owned by MVC
            var memoryStream = new MemoryStream();

            await response.Content.CopyToAsync(
                memoryStream,
                cancellationToken);

            memoryStream.Position = 0;

            Response.Headers.ContentDisposition =
                new System.Net.Mime.ContentDisposition
                {
                    Inline = true,
                    FileName = fileName
                }.ToString();

            response.Dispose();

            return File(
                memoryStream,
                contentType);
        }
        private async Task<IActionResult> ForwardErrorAsync(
            HttpResponseMessage response,
            string fallbackMessage,
            CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    new { message = "You are not authorized to perform this action." });
            }

            string? message = null;

            try
            {
                var problem = await response.Content
                    .ReadFromJsonAsync<ApiErrorBody>(cancellationToken: cancellationToken);

                message = problem?.Message ?? problem?.Title ?? problem?.Detail;
            }
            catch
            {
                // Response body wasn't JSON in the expected shape; fall back below.
            }

            return StatusCode(
                (int)response.StatusCode,
                new { message = string.IsNullOrWhiteSpace(message) ? fallbackMessage : message });
        }

        /// <summary>Loosely matches both plain error bodies and ProblemDetails.</summary>
        private class ApiErrorBody
        {
            public string? Message { get; set; }
            public string? Title { get; set; }
            public string? Detail { get; set; }
        }
    }
}
