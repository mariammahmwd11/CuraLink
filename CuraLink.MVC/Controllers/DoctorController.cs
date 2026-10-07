using CuraLink.MVC.Models.Doctors;
using CuraLink.MVC.Models.Patients;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CuraLink.MVC.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ClinicApiClient _clinicApiClient;
        private readonly DoctorScheduleApiClient _scheduleApiClient;
        private readonly AppointmentApiClient _appointmentApiClient;
        // Despite its name, GetProfileAsync/UpdateProfileAsync call the shared /api/profile endpoints.
        private readonly PatientApiClient _profileApiClient;
        private readonly DoctorPatientApiClient _doctorPatientApiClient;
        DoctorClinicAssistantApiClient doctorClinicAssistantApiClient;

        public DoctorController(
            ClinicApiClient clinicApiClient,
            DoctorScheduleApiClient scheduleApiClient,
            AppointmentApiClient appointmentApiClient,
            PatientApiClient profileApiClient,
            DoctorPatientApiClient doctorPatientApiClient,
            DoctorClinicAssistantApiClient doctorClinicAssistantApiClient)
        {
            _clinicApiClient = clinicApiClient;
            _scheduleApiClient = scheduleApiClient;
            _appointmentApiClient = appointmentApiClient;
            _profileApiClient = profileApiClient;
            _doctorPatientApiClient = doctorPatientApiClient;
            this.doctorClinicAssistantApiClient = doctorClinicAssistantApiClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
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
        // Dashboard
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var model = new DoctorDashboardViewModel
            {
                DoctorDisplayName = GetDoctorDisplayName()
            };

            try
            {
                model.Clinics = await _clinicApiClient.GetMyClinicsAsync();
                model.TotalClinics = model.Clinics.Count;
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Numbers are computed from the doctor's real appointments.
            try
            {
                var bookings = await GetRealBookingsAsync(cancellationToken);
                var today = DateTime.Today;

                model.TodaysAppointments = bookings.Count(a => a.Date.Date == today);
                model.UpcomingAppointments = bookings.Count(a => a.Date.Date > today && !IsStatus(a, "Completed"));
                model.TotalPatients = bookings
                    .Select(a => a.PatientName.Trim())
                    .Where(n => n.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                // Keep the dashboard usable: the numbers stay at 0 if appointments can't be loaded.
            }

            return View(model);
        }

        // =========================================================
        // Patients (everyone who has booked with this doctor)
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Patients(
    CancellationToken cancellationToken)
        {
            try
            {
                var patients =
                    await _doctorPatientApiClient.GetMyPatientsAsync(
                        cancellationToken);

                return View(patients);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to load your patients. Please try again.";

                return View(new List<DoctorPatientViewModel>());
            }
        }
       

        [HttpGet]
        public async Task<IActionResult> Patient(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "This patient could not be found.";
                return RedirectToAction(nameof(Patients));
            }

            try
            {
                var profile = await _doctorPatientApiClient.GetPatientProfileAsync(id, cancellationToken);
                return View(profile);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "This patient could not be found.";
                return RedirectToAction(nameof(Patients));
            }
            catch (Exception)
            {
                // No exception details are shown to the user.
                TempData["ErrorMessage"] = "Unable to load this patient's profile.";
                return RedirectToAction(nameof(Patients));
            }
        }

        // ---------- Medical documents (the JWT stays server-side) ----------

        // Types that are safe to render inline in the browser; anything else is forced to download.
        private static readonly HashSet<string> InlineDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
{
    "application/pdf", "image/jpeg", "image/jpg", "image/png"
};

        [HttpGet]
        public async Task<IActionResult> ViewMedicalDocument(
            Guid patientId,
            int documentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var file = await _doctorPatientApiClient
                    .ViewMedicalDocumentAsync(patientId, documentId, cancellationToken);

                Response.Headers["X-Content-Type-Options"] = "nosniff";
                Response.Headers["Cache-Control"] = "private, no-store";

                if (!InlineDocumentTypes.Contains(file.ContentType))
                {
                    return File(file.Content, "application/octet-stream", file.FileName ?? $"document-{documentId}");
                }

                // No download file name -> the browser displays it inline (PDF viewer / image).
                return File(file.Content, file.ContentType);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to open the medical document.";
                return RedirectToAction(nameof(Patient), new { id = patientId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMedicalDocument(
            Guid patientId,
            int documentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var file = await _doctorPatientApiClient
                    .DownloadMedicalDocumentAsync(patientId, documentId, cancellationToken);

                Response.Headers["X-Content-Type-Options"] = "nosniff";
                Response.Headers["Cache-Control"] = "private, no-store";

                return File(file.Content, file.ContentType, file.FileName ?? $"document-{documentId}");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Unable to download the medical document.";
                return RedirectToAction(nameof(Patient), new { id = patientId });
            }
        }

        // =========================================================
        // Profile (shared /api/profile endpoints)
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Profile(CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _profileApiClient.GetProfileAsync(cancellationToken);
                profile.Email = GetEmail(); // API doesn't return email
                return View(profile);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                return View(new PatientProfileViewModel { Email = GetEmail() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _profileApiClient.GetProfileAsync(cancellationToken);
                profile.Email = GetEmail();
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
        public async Task<IActionResult> EditProfile(
            PatientProfileViewModel model,
            CancellationToken cancellationToken)
        {
            const long maxFileSize = 5 * 1024 * 1024;
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };

            if (model.ProfilePhoto is { Length: > 0 })
            {
                if (model.ProfilePhoto.Length > maxFileSize)
                {
                    ModelState.AddModelError(string.Empty, "Profile photo must not exceed 5 MB.");
                }
                else if (!allowedContentTypes.Contains(model.ProfilePhoto.ContentType))
                {
                    ModelState.AddModelError(string.Empty, "Only JPG, JPEG, PNG, and WebP images are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Email = GetEmail();
                return View(model);
            }

            try
            {
                await _profileApiClient.UpdateProfileAsync(
                    model.PhoneNumber,
                    model.Bio,
                    model.ProfilePhoto,
                    cancellationToken);

                TempData["SuccessMessage"] = "Your profile was updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Email = GetEmail();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong while updating your profile.");
                model.Email = GetEmail();
                return View(model);
            }
        }

        // =========================================================
        // Availability / Schedule
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Schedule(CancellationToken cancellationToken)
        {
            try
            {
                var model = await _scheduleApiClient.GetScheduleAsync(cancellationToken);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString()); // مؤقت للتشخيص فقط
                return View(new DoctorScheduleViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(
            DoctorScheduleViewModel model,
            CancellationToken cancellationToken)
        {
            // Validate only the days the doctor actually enabled.
            for (var i = 0; i < model.Days.Count; i++)
            {
                var day = model.Days[i];

                if (!day.IsEnabled)
                {
                    continue;
                }

                if (day.StartTime is null || day.EndTime is null)
                {
                    ModelState.AddModelError(
                        $"Days[{i}]",
                        $"{day.DayOfWeek}: start and end time are required.");
                    continue;
                }

                if (day.StartTime >= day.EndTime)
                {
                    ModelState.AddModelError(
                        $"Days[{i}]",
                        $"{day.DayOfWeek}: start time must be before end time.");
                }

                if (day.SlotDurationMinutes <= 0)
                {
                    ModelState.AddModelError(
                        $"Days[{i}]",
                        $"{day.DayOfWeek}: slot duration must be greater than zero.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _scheduleApiClient.UpdateScheduleAsync(model.Days, cancellationToken);

                TempData["SuccessMessage"] = "Your schedule was saved successfully.";
                return RedirectToAction(nameof(Schedule));
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Something went wrong while saving your schedule.");
                return View(model);
            }
        }
        // =========================================================
// Assistants (receptionists of the doctor's clinics)
// =========================================================

[HttpGet]
public async Task<IActionResult> Assistants(Guid? clinicId, CancellationToken cancellationToken)
{
    var model = new DoctorAssistantsViewModel();

    try
    {
        model.Clinics = await LoadClinicOptionsAsync();

        if (model.Clinics.Count == 0)
            return View(model);

        var selected = model.Clinics.FirstOrDefault(c => c.Id == clinicId) ?? model.Clinics[0];
        model.SelectedClinicId = selected.Id;
        model.SelectedClinicName = selected.Name;

        model.Assistants = await doctorClinicAssistantApiClient.GetAssistantsAsync(selected.Id, cancellationToken);
        return View(model);
    }
    catch (UnauthorizedAccessException)
    {
        return RedirectToAction("Login", "Auth");
    }
    catch (Exception)
    {
        TempData["ErrorMessage"] = "Unable to load your assistants. Please try again.";
        return View(model);
    }
}

[HttpGet]
public async Task<IActionResult> AddAssistant(Guid clinicId)
{
    try
    {
        var clinic = (await LoadClinicOptionsAsync()).FirstOrDefault(c => c.Id == clinicId);

        if (clinic is null)
        {
            TempData["ErrorMessage"] = "Clinic not found.";
            return RedirectToAction(nameof(Assistants));
        }

        return View(new AddAssistantViewModel { ClinicId = clinic.Id, ClinicName = clinic.Name });
    }
    catch (UnauthorizedAccessException)
    {
        return RedirectToAction("Login", "Auth");
    }
    catch (Exception)
    {
        TempData["ErrorMessage"] = "Unable to open the page. Please try again.";
        return RedirectToAction(nameof(Assistants));
    }
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddAssistant(
    AddAssistantViewModel model,
    CancellationToken cancellationToken)
{
    if (!ModelState.IsValid)
        return View(model);

    try
    {
        var error = await doctorClinicAssistantApiClient.InviteAsync(
            model.ClinicId, model.Email.Trim(), cancellationToken);

        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = "Invitation sent successfully.";
        return RedirectToAction(nameof(Assistants), new { clinicId = model.ClinicId });
    }
    catch (UnauthorizedAccessException)
    {
        return RedirectToAction("Login", "Auth");
    }
    catch (HttpRequestException)
    {
        ModelState.AddModelError(string.Empty, "Could not reach the server. Please try again later.");
        return View(model);
    }
    catch (Exception)
    {
        ModelState.AddModelError(string.Empty, "Something went wrong while sending the invitation.");
        return View(model);
    }
}

private async Task<List<AssistantClinicOption>> LoadClinicOptionsAsync()
{
    var clinics = await _clinicApiClient.GetMyClinicsAsync();

    // ASSUMPTION: the clinic items expose Id and ClinicName. Adjust if yours differ.
    return clinics.Select(c => new AssistantClinicOption(c.Id, c.ClinicName)).ToList();
}

        // =========================================================
        // Helpers
        // =========================================================

        private static bool IsStatus(MyAppointmentViewModel a, string status) =>
            string.Equals(a.Status?.Trim(), status, StringComparison.OrdinalIgnoreCase);

        // Appointments that count as real bookings: not Pending (unpaid) and not Cancelled.
        private async Task<List<MyAppointmentViewModel>> GetRealBookingsAsync(CancellationToken cancellationToken)
        {
            var all = await _appointmentApiClient.GetMyAppointmentsAsync(cancellationToken);

            return all
                .Where(a => !IsStatus(a, "Pending")
                         && !IsStatus(a, "Cancelled")
                         && !IsStatus(a, "Canceled"))
                .ToList();
        }

        private string GetEmail() =>
            User.FindFirst("Email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? string.Empty;

        private string GetDoctorDisplayName()
        {
            var name = User.FindFirst(ClaimTypes.GivenName)?.Value
                ?? User.FindFirst("FirstName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(name) ? "Doctor" : name;
        }
    }
}