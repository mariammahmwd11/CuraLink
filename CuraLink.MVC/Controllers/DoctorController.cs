using System.Security.Claims;
using CuraLink.MVC.Models.Doctors;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ClinicApiClient _clinicApiClient;

        public DoctorController(ClinicApiClient clinicApiClient)
        {
            _clinicApiClient = clinicApiClient;
        }

        /// <summary>
        /// Alias only. ClinicController.Create() already does
        /// RedirectToAction("Index", "Doctor") after a successful create —
        /// this keeps that existing redirect working without touching
        /// ClinicController. Real dashboard lives in Dashboard().
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        
        public async Task<IActionResult> Dashboard()
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

            // TODO: replace with real counts once appointment/patient endpoints
            // exist for the doctor role. Intentionally left at 0, not faked.
            model.TodaysAppointments = 0;
            model.UpcomingAppointments = 0;
            model.TotalPatients = 0;

            return View(model);
        }

        private string GetDoctorDisplayName()
        {
            // TODO: confirm the exact claim type used at sign-in for the
            // doctor's first/display name and trim this fallback chain
            // down to the one that's actually populated.
            var name = User.FindFirst(ClaimTypes.GivenName)?.Value
                ?? User.FindFirst("FirstName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(name) ? "Doctor" : name;
        }
    }
}
