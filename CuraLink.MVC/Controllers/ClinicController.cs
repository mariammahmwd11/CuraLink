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
    }
}