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

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var model = new CreatePrescriptionViewModel();

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

            return RedirectToAction(nameof(Create));
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