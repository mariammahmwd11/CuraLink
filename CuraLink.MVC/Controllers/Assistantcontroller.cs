using CuraLink.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    // Clinic assistant = a user with the Receptionist role who is linked to a clinic.
    [Authorize(Roles = "Receptionist")]
    public class AssistantController : Controller
    {
        // GET /Assistant
        [HttpGet]
        public IActionResult Index()
        {
            // TODO (when the backend is ready): call the API with the "AccessToken" claim
            // (same pattern as ChatController) and fill this model:
            //   GET /api/clinic-assistants/me/dashboard
            var model = new AssistantDashboardViewModel
            {
                ClinicName = "" // comes from the API
            };

            return View(model);
        }
    }
}