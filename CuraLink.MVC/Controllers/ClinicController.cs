using CuraLink.MVC.Models.Clinics;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class ClinicController : Controller
    {
        private readonly ClinicApiClient _clinicApiClient;

        public ClinicController(
            ClinicApiClient clinicApiClient)
        {
            _clinicApiClient = clinicApiClient;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateClinicViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateClinicViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clinicApiClient.CreateClinicAsync(model);

                TempData["SuccessMessage"] =
                    "Clinic created successfully.";

                return RedirectToAction("Index", "Doctor");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
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
                    "Something went wrong while creating the clinic.");

                return View(model);
            }
        }

        // =========================================================
        // Edit
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var clinic = await _clinicApiClient.GetClinicAsync(id);

                if (clinic is null)
                {
                    TempData["ErrorMessage"] = "Clinic not found.";
                    return RedirectToAction("Dashboard", "Doctor");
                }

                ViewBag.ClinicId = id;

                return View(new CreateClinicViewModel
                {
                    ClinicName = clinic.ClinicName,
                    Address = clinic.Address,
                    PhoneNumber = clinic.PhoneNumber,
                    ConsultationPrice = clinic.ConsultationPrice
                });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateClinicViewModel model)
        {
            ViewBag.ClinicId = id;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clinicApiClient.UpdateClinicAsync(id, model);

                TempData["SuccessMessage"] = "Clinic updated successfully.";
                return RedirectToAction("Dashboard", "Doctor");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Clinic not found.";
                return RedirectToAction("Dashboard", "Doctor");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
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
                    "Something went wrong while updating the clinic.");
                return View(model);
            }
        }

        // =========================================================
        // Delete
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _clinicApiClient.DeleteClinicAsync(id);

                TempData["SuccessMessage"] = "Clinic deleted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Clinic not found. It may have already been deleted.";
            }
            catch (InvalidOperationException ex)
            {
                // e.g. clinic still has upcoming appointments
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong while deleting the clinic.";
            }

            return RedirectToAction("Dashboard", "Doctor");
        }
    }
}
