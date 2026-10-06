using CuraLink.MVC.Models.Prescriptions;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class PrescriptionController : Controller
    {
        private readonly PrescriptionApiClient _prescriptionApiClient;

        public PrescriptionController(PrescriptionApiClient prescriptionApiClient)
        {
            _prescriptionApiClient = prescriptionApiClient;
        }

        // =========================================================
        // List — where the "Export PDF" action lives for each row
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var prescriptions = await _prescriptionApiClient
                    .GetMyPrescriptionsAsync(cancellationToken);

                return View(prescriptions);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Your prescriptions could not be loaded right now. Please refresh the page.";

                return View(new List<DoctorPrescriptionListItemViewModel>());
            }
        }

        // =========================================================
        // Export PDF — downloads the file, never navigates to raw JSON
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var (bytes, fileName) = await _prescriptionApiClient
                    .DownloadPrescriptionPdfAsync(id, cancellationToken);

                return File(bytes, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Covers both "no token on principal" and API 401/403 —
                // 403 specifically means "not this doctor's prescription".
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewPdf(
    Guid id,
    CancellationToken cancellationToken)
        {
            try
            {
                var (bytes, fileName) = await _prescriptionApiClient
                    .DownloadPrescriptionPdfAsync(id, cancellationToken);

                // Open PDF in browser instead of downloading it
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

                return File(bytes, "application/pdf");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create(
     Guid? patientId,
     CancellationToken cancellationToken)
        {
            var model = new CreatePrescriptionViewModel();

            try
            {
                model.Patients = await _prescriptionApiClient
                    .GetMyPatientsAsync(cancellationToken);

                if (patientId.HasValue)
                {
                    var patient = model.Patients.FirstOrDefault(
                        p => p.PatientId == patientId.Value);

                    if (patient != null)
                    {
                        model.PatientUserId = patient.PatientUserId;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your patient list could not be loaded right now. Please refresh the page.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePrescriptionViewModel model,
            CancellationToken cancellationToken)
        {
            // Rows the doctor added but left completely blank are dropped rather
            // than reported as five validation errors each.
            model.Items = model.Items?
                .Where(i => !i.IsEmpty())
                .ToList() ?? new List<PrescriptionItemViewModel>();

            ModelState.Clear();
            TryValidateModel(model);

            if (model.Items.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Items),
                    "Add at least one medication.");
            }

            if (!ModelState.IsValid)
            {
                return await RedisplayAsync(model, cancellationToken);
            }

            PrescriptionCreateResult result;

            try
            {
                result = await _prescriptionApiClient
                    .CreatePrescriptionAsync(model, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The server could not be reached. Please try again.");

                return await RedisplayAsync(model, cancellationToken);
            }

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "The prescription could not be saved.");

                return await RedisplayAsync(model, cancellationToken);
            }

            TempData["PrescriptionSuccess"] = "Prescription created successfully.";

            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// The patient list is never posted back, so it has to be refetched
        /// before the form is rendered again.
        /// </summary>
        private async Task<IActionResult> RedisplayAsync(
            CreatePrescriptionViewModel model,
            CancellationToken cancellationToken)
        {
            if (model.Items.Count == 0)
            {
                model.Items.Add(new PrescriptionItemViewModel());
            }

            try
            {
                model.Patients = await _prescriptionApiClient
                    .GetMyPatientsAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                model.Patients = new List<DoctorPatientOptionViewModel>();
            }

            return View(model);
        }
    }
}
