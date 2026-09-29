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

        public DoctorController(
            ClinicApiClient clinicApiClient,
            DoctorScheduleApiClient scheduleApiClient,AppointmentApiClient appointmentApiClient)
        {
            _clinicApiClient = clinicApiClient;
            _scheduleApiClient = scheduleApiClient;
            _appointmentApiClient = appointmentApiClient;
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

            model.TodaysAppointments = 0;
            model.UpcomingAppointments = 0;
            model.TotalPatients = 0;

            return View(model);
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

        private string GetDoctorDisplayName()
        {
            var name = User.FindFirst(ClaimTypes.GivenName)?.Value
                ?? User.FindFirst("FirstName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(name) ? "Doctor" : name;
        }
    }
}