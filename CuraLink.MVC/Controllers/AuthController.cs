using CuraLink.MVC.Models.Auth;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CuraLink.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiClient _authApiClient;

        public AuthController(AuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _authApiClient.LoginAsync(model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterPatient(
       RegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _authApiClient.RegisterPatientAsync(model);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");

                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please try again.");

                return View(model);
            }

            return RedirectToAction(nameof(Login));
        }
    }
    }
