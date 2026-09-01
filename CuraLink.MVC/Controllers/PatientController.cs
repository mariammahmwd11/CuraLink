using System.Security.Claims;
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

        public PatientController(PatientApiClient patientApiClient)
        {
            _patientApiClient = patientApiClient;
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var documents =
                    await _patientApiClient.GetMedicalDocumentsAsync();

                var model = new PatientDashboardViewModel
                {
                    Profile = GetProfileFromClaims(),
                    MedicalDocumentsCount = documents.Count,
                    UpcomingAppointmentsCount = 0
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
                    UpcomingAppointmentsCount = 0
                };

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View(GetProfileFromClaims());
        }

        [HttpGet]
        public async Task<IActionResult> MedicalRecords()
        {
            // Always returns an empty list today — see PatientApiClient.GetMedicalDocumentsAsync.
            var documents = await _patientApiClient.GetMedicalDocumentsAsync();
            return View(documents);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View(new UploadMedicalDocumentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadMedicalDocumentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };

            if (model.File.Length > maxFileSize)
            {
                ModelState.AddModelError(string.Empty, "File size must not exceed 10 MB.");
                return View(model);
            }

            if (!allowedContentTypes.Contains(model.File.ContentType))
            {
                ModelState.AddModelError(string.Empty, "Only PDF, JPG, JPEG, and PNG files are allowed.");
                return View(model);
            }

            try
            {
                await _patientApiClient.UploadMedicalDocumentAsync(model.File);

                TempData["SuccessMessage"] = "Your medical document was uploaded successfully.";
                return RedirectToAction(nameof(MedicalRecords));
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
                ModelState.AddModelError(string.Empty, "Something went wrong while uploading the document.");
                return View(model);
            }
        }

        private PatientProfileViewModel GetProfileFromClaims()
        {
            var email = User.FindFirst("Email")?.Value
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