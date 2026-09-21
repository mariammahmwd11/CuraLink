using System.Security.Claims;
using CuraLink.MVC.Models.Notifications;
using CuraLink.MVC.Models.Patients;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly PatientApiClient _patientApiClient;
        private readonly NotificationApiClient _notificationApiClient;

        public PatientController(
            PatientApiClient patientApiClient,
            NotificationApiClient notificationApiClient)
        {
            _patientApiClient = patientApiClient;
            _notificationApiClient = notificationApiClient;
        }

        // =========================================================
        // Dashboard
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            try
            {
                var documents =
                    await _patientApiClient.GetMedicalDocumentsAsync();

                // Server-side source of truth for "is this patient already
                // subscribed to push?" — avoids relying only on the
                // browser's Notification.permission, which can drift out
                // of sync with what the backend actually has stored.
                var notificationsEnabled = await _notificationApiClient
                    .GetSubscriptionStatusAsync(cancellationToken);

                var model = new PatientDashboardViewModel
                {
                    Profile = GetProfileFromClaims(),
                    MedicalDocumentsCount = documents.Count,
                    UpcomingAppointmentsCount = 0,
                    NotificationsEnabled = notificationsEnabled
                };

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                var model = new PatientDashboardViewModel
                {
                    Profile = GetProfileFromClaims(),
                    MedicalDocumentsCount = 0,
                    UpcomingAppointmentsCount = 0,
                    NotificationsEnabled = false
                };

                return View(model);
            }
        }

        // =========================================================
        // Profile
        // =========================================================

        [HttpGet]
        public IActionResult Profile()
        {
            return View(GetProfileFromClaims());
        }

        // =========================================================
        // Medical Records
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MedicalRecords()
        {
            try
            {
                var documents =
                    await _patientApiClient.GetMedicalDocumentsAsync();

                return View(documents);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(new List<MedicalDocumentViewModel>());
            }
        }

        // =========================================================
        // Doctors Directory
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Doctors(
            string? doctorName,
            string? specialty,
            string? governorate,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var model = new DoctorSearchViewModel
            {
                DoctorName = doctorName,
                Specialty = specialty,
                Governorate = governorate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result =
                    await _patientApiClient.SearchDoctorsAsync(
                        doctorName,
                        specialty,
                        governorate,
                        pageNumber,
                        pageSize);

                model.Doctors = result.Items;
                model.PageNumber = result.PageNumber;
                model.PageSize = result.PageSize;
                model.TotalCount = result.TotalCount;
                model.TotalPages = result.TotalPages;

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "We couldn't load doctors right now. Please try again.");

                return View(model);
            }
        }

        // =========================================================
        // Upload Medical Document
        // =========================================================

        [HttpGet]
        public IActionResult Upload()
        {
            return View(
                new UploadMedicalDocumentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            UploadMedicalDocumentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            const long maxFileSize = 10 * 1024 * 1024;

            var allowedContentTypes = new[]
            {
                "application/pdf",
                "image/jpeg",
                "image/jpg",
                "image/png"
            };

            if (model.File.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "File size must not exceed 10 MB.");

                return View(model);
            }

            if (!allowedContentTypes.Contains(
                    model.File.ContentType))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Only PDF, JPG, JPEG, and PNG files are allowed.");

                return View(model);
            }

            try
            {
                await _patientApiClient
                    .UploadMedicalDocumentAsync(model.File);

                TempData["SuccessMessage"] =
                    "Your medical document was uploaded successfully.";

                return RedirectToAction(
                    nameof(MedicalRecords));
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Something went wrong while uploading the document.");

                return View(model);
            }
        }

        // =========================================================
        // Notification Subscription (Web Push)
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> RegisterNotificationSubscription(
            [FromBody] RegisterNotificationSubscriptionViewModel model)
        {
            var success =
                await _notificationApiClient.RegisterSubscriptionAsync(
                    model.Endpoint,
                    model.P256DH,
                    model.Auth);

            if (!success)
            {
                return BadRequest();
            }

            return Ok();
        }

        // =========================================================
        // In-app notifications (bell dropdown) — JSON endpoints
        // consumed by wwwroot/js/patient/notifications.js.
        // Same convention as RegisterNotificationSubscription above:
        // this MVC action proxies to the API using the patient's
        // stored JWT, the browser never talks to the API directly.
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Notifications(
            CancellationToken cancellationToken)
        {
            var notifications =
                await _notificationApiClient.GetNotificationsAsync(cancellationToken);

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> UnreadNotificationCount(
            CancellationToken cancellationToken)
        {
            var count =
                await _notificationApiClient.GetUnreadCountAsync(cancellationToken);

            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(
            Guid id,
            CancellationToken cancellationToken)
        {
            var success =
                await _notificationApiClient.MarkAsReadAsync(id, cancellationToken);

            return success ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead(
            CancellationToken cancellationToken)
        {
            var success =
                await _notificationApiClient.MarkAllAsReadAsync(cancellationToken);

            return success ? Ok() : NotFound();
        }

        // =========================================================
        // Claims
        // =========================================================

        private PatientProfileViewModel GetProfileFromClaims()
        {
            var email =
                User.FindFirst("Email")?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? string.Empty;

            return new PatientProfileViewModel
            {
                Email = email
            };
        }
    }
}
