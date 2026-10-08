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

        private readonly AppointmentApiClient _appointmentApiClient;

        public PatientController(
            PatientApiClient patientApiClient,
            NotificationApiClient notificationApiClient,
            AppointmentApiClient appointmentApiClient)
        {
            _patientApiClient = patientApiClient;
            _notificationApiClient = notificationApiClient;
            _appointmentApiClient = appointmentApiClient;
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
                var appointments =
          await _appointmentApiClient.GetMyAppointmentsAsync(cancellationToken);

                var now = DateTime.Now;

                var upcomingAppointmentsCount =
      appointments.Count(a =>
          a.Status != "Cancelled" &&
          a.Status != "Completed" &&
          (
              a.Date.Date > now.Date ||
              (
                  a.Date.Date == now.Date &&
                  a.EndTime > now.TimeOfDay
              )
          ));
                var model = new PatientDashboardViewModel
                {
                    Profile = GetProfileFromClaims(),
                    MedicalDocumentsCount = documents.Count,
                    UpcomingAppointmentsCount = upcomingAppointmentsCount,
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
        [HttpGet]
        public async Task<IActionResult> Appointments(CancellationToken cancellationToken)
        {
            try
            {
                var appointments = await _appointmentApiClient.GetMyAppointmentsAsync(cancellationToken);
                return View(appointments);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to load your appointments. Please try again.";
                return View(new List<MyAppointmentViewModel>());
            }
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
        [HttpGet]
        public async Task<IActionResult> ViewMedicalDocument(
    int id,
    CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _patientApiClient.GetMedicalDocumentAsync(
                        id,
                        cancellationToken);

                return File(
                    result.Stream,
                    result.ContentType);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to open this medical document.";

                return RedirectToAction(nameof(MedicalRecords));
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadMedicalDocument(
    int id,
    CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _patientApiClient.DownloadMedicalDocumentAsync(
                        id,
                        cancellationToken);

                return File(
                    result.Stream,
                    result.ContentType,
                    result.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to download this medical document.";

                return RedirectToAction(nameof(MedicalRecords));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalDocument(
    int id,
    CancellationToken cancellationToken)
        {
            try
            {
                await _patientApiClient.DeleteMedicalDocumentAsync(
                    id,
                    cancellationToken);

                TempData["SuccessMessage"] =
                    "Medical document deleted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] =
                    "Medical document not found.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete this medical document.";
            }

            return RedirectToAction(nameof(MedicalRecords));
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
        // Edit Profile
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _patientApiClient.GetProfileAsync(cancellationToken);
                profile.Email = GetProfileFromClaims().Email;

                return View(profile);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Profile));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PatientProfileViewModel model,
            CancellationToken cancellationToken)
        {
            const long maxFileSize = 5 * 1024 * 1024;

            var allowedContentTypes = new[]
            {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

            if (model.ProfilePhoto is { Length: > 0 })
            {
                if (model.ProfilePhoto.Length > maxFileSize)
                {
                    ModelState.AddModelError(string.Empty, "Profile photo must not exceed 5 MB.");
                }
                else if (!allowedContentTypes.Contains(model.ProfilePhoto.ContentType))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Only JPG, JPEG, PNG, and WebP images are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Email = GetProfileFromClaims().Email;
                return View(model);
            }

            try
            {
                await _patientApiClient.UpdateProfileAsync(
                    model.PhoneNumber,
                    model.Bio,
                    model.ProfilePhoto,
                    cancellationToken);

                TempData["SuccessMessage"] = "Your profile was updated successfully.";

                // Redirect (not View) so Profile() re-fetches fresh data —
                // same PRG pattern as Upload().
                return RedirectToAction(nameof(Profile));
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Email = GetProfileFromClaims().Email;
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Something went wrong while updating your profile.");
                model.Email = GetProfileFromClaims().Email;
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Profile(CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _patientApiClient.GetProfileAsync(cancellationToken);
                profile.Email = GetProfileFromClaims().Email; // API doesn't return email

                return View(profile);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                // Same defensive fallback style as Dashboard(): show what we can
                // from claims rather than a hard error page.
                return View(GetProfileFromClaims());
            }
        }
        // =========================================================
        // Doctor Details / Appointment Booking
        // =========================================================

        // NOTE: no GET-by-id doctor endpoint exists yet, so the doctor's
        // display info is carried here as query values from the Doctors
        // search results page rather than re-fetched. Once a real
        // "get doctor by id" endpoint exists, replace this with a call to it.
        [HttpGet]
        public IActionResult DoctorDetails(
            Guid id,
              Guid clinicId,
            string? doctorName,
            string? specialty,
            string? address,
            string? governorate,
            decimal consultationPrice,
            double rating, string? profilePhoto)
        {
            var model = new DoctorBookingViewModel
            {
                DoctorId = id,
                ClinicId = clinicId,
                DoctorName = string.IsNullOrWhiteSpace(doctorName) ? "Doctor" : doctorName,
                Specialty = specialty,
                Address = address,
                Governorate = governorate,
                ConsultationPrice = consultationPrice,
                Rating = rating,
                ProfilePhoto = profilePhoto
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AvailableSlots(
     Guid doctorId,
     DateOnly date,
     CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _appointmentApiClient.GetAvailableSlotsAsync(
                        doctorId,
                        date,
                        cancellationToken);

                return Json(new
                {
                    success = true,

                    isDoctorAvailable =
                        result.IsDoctorAvailable,

                    slots = result.Slots.Select(s => new
                    {
                        startTime =
                            s.StartTime.ToString(@"hh\:mm"),

                        endTime =
                            s.EndTime.ToString(@"hh\:mm")
                    })
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new
                {
                    success = false,
                    error =
                        "Your session has expired. Please log in again."
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    error =
                        "We couldn't load available slots right now."
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> BookAppointment(
     [FromBody] BookAppointmentRequest request,
     CancellationToken cancellationToken)
        {
            try
            {
                // ========================================================
                // 1. Create Pending Appointment
                // ========================================================

                var appointmentId =
                    await _appointmentApiClient.BookAppointmentAsync(
                        request.DoctorId,
                        request.ClinicId,
                        request.Date,
                        request.StartTime,
                        request.EndTime,
                        cancellationToken);

                // ========================================================
                // 2. Create Stripe Checkout Session
                // ========================================================

                string checkoutUrl;

                try
                {
                    checkoutUrl =
                        await _appointmentApiClient
                            .CreateCheckoutSessionAsync(
                                appointmentId,
                                cancellationToken);
                }
                catch
                {
                    // Payment session failed.
                    // Cancel the pending appointment so the slot
                    // becomes available again.

                    try
                    {
                        await _appointmentApiClient
                            .CancelPendingAppointmentAsync(
                                appointmentId,
                                CancellationToken.None);
                    }
                    catch
                    {
                        // Best effort.
                    }

                    throw;
                }

                // ========================================================
                // 3. Return Stripe URL to JavaScript
                // ========================================================

                return Json(new
                {
                    success = true,
                    appointmentId,
                    checkoutUrl
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new
                {
                    success = false,
                    error = "Your session has expired. Please log in again."
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "Something went wrong while booking your appointment."
                });
            }
        }
        public class BookAppointmentRequest
        {
            public Guid DoctorId { get; set; }
            public Guid ClinicId { get; set; }
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
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
