
using CuraLink.MVC.Models;
using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers
{
    public class ClinicAssistantController : Controller
    {
        private readonly ClinicAssistantApiService _api;
        private readonly ILogger<ClinicAssistantController> _logger;

        public ClinicAssistantController(
            ClinicAssistantApiService api,
            ILogger<ClinicAssistantController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET /assistant/invitation/accept?token=...
        // GET /ClinicAssistant/Invitation?token=...
        [HttpGet("assistant/invitation/accept")]
        [HttpGet("ClinicAssistant/Invitation")]
        [AllowAnonymous]
        public async Task<IActionResult> Invitation(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return View(new InvitationViewModel
                {
                    ErrorMessage = "This invitation link is invalid."
                });
            }

            var preview = await _api.PreviewAsync(token);

            return View(BuildInvitationModel(token, preview));
        }

        // GET /ClinicAssistant/Register?token=...
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return View(
                    "Invitation",
                    new InvitationViewModel
                    {
                        ErrorMessage = "This invitation link is invalid."
                    });
            }

            var preview = await _api.PreviewAsync(token);

            if (!preview.Success || preview.Data == null)
            {
                return View(
                    "Invitation",
                    BuildInvitationModel(token, preview));
            }

            // Account already exists.
            if (preview.Data.IsRegistered)
            {
                return RedirectToAction(
                    nameof(Invitation),
                    new { token });
            }

            return View(new RegisterInvitationViewModel
            {
                Token = token,
                Email = preview.Data.Email,
                ClinicName = preview.Data.ClinicName
            });
        }

        // POST /ClinicAssistant/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterInvitationViewModel model)
        {
            var password = model.Password;

            // Never echo password values back to the page.
            model.Password = "";
            model.ConfirmPassword = "";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.RegisterAsync(
                model.Token,
                model.FirstName.Trim(),
                model.LastName.Trim(),
                model.Phone.Trim(),
                password);

            if (!result.Success)
            {
                model.ErrorMessage =
                    ClinicAssistantApiService.FriendlyMessage(
                        result.StatusCode,
                        result.RawMessage,
                        allowValidationText: true);

                return View(model);
            }

            TempData["InvitationMessage"] =
                "Account created successfully. Please log in to accept the invitation.";

            return RedirectToAction(
                "Login",
                "Auth",
                new
                {
                    invitationToken = model.Token
                });
        }

        // POST /ClinicAssistant/Accept
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return View(
                    "Invitation",
                    new InvitationViewModel
                    {
                        ErrorMessage = "This invitation link is invalid."
                    });
            }

            var preview = await _api.PreviewAsync(token);

            if (!preview.Success || preview.Data == null)
            {
                return View(
                    "Invitation",
                    BuildInvitationModel(token, preview));
            }

            var accessToken = User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning(
                    "Accept invitation: no access token for the current user.");

                return RedirectToAction(
                    "Login",
                    "Auth",
                    new
                    {
                        invitationToken = token
                    });
            }

            var result = await _api.AcceptAsync(
                token,
                accessToken);

            if (!result.Success)
            {
                var model = BuildInvitationModel(
                    token,
                    preview);

                model.ErrorMessage =
                    ClinicAssistantApiService.FriendlyMessage(
                        result.StatusCode,
                        result.RawMessage);

                return View("Invitation", model);
            }

            // The current JWT does not contain the new Receptionist role.
            // Sign out so the user can log in again and receive a fresh JWT.
            try
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Sign out after accepting invitation failed.");
            }

            return View(
                "Accepted",
                new InvitationAcceptedViewModel
                {
                    ClinicName = preview.Data.ClinicName
                });
        }

        private InvitationViewModel BuildInvitationModel(
            string token,
            ApiResult<InvitationPreviewDto> preview)
        {
            if (!preview.Success || preview.Data == null)
            {
                return new InvitationViewModel
                {
                    Token = token,
                    ErrorMessage =
                        ClinicAssistantApiService.FriendlyMessage(
                            preview.StatusCode,
                            preview.RawMessage)
                };
            }

            return new InvitationViewModel
            {
                Token = token,
                Email = preview.Data.Email,
                ClinicName = preview.Data.ClinicName,
                IsRegistered = preview.Data.IsRegistered,
                IsAuthenticated =
                    User.Identity?.IsAuthenticated == true
            };
        }
    }
}
