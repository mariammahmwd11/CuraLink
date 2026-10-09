using CuraLink.MVC.Models;
using CuraLink.MVC.Models.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CuraLink.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new LandingViewModel();

            if (User.Identity?.IsAuthenticated == true)
            {
                // Same priority order as AuthController.Login
                if (User.IsInRole("Patient"))
                    (model.DashboardController, model.DashboardAction) = ("Patient", "Dashboard");
                else if (User.IsInRole("Doctor"))
                    (model.DashboardController, model.DashboardAction) = ("Doctor", "Dashboard");
                else if (User.IsInRole("Admin"))
                    (model.DashboardController, model.DashboardAction) = ("Admin", "PendingDoctors");
                else if (User.IsInRole("Receptionist"))
                    (model.DashboardController, model.DashboardAction) = ("Assistant", "Index");
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}