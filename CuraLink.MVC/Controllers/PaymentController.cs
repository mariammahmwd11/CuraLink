using CuraLink.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.MVC.Controllers;

[Authorize(Roles = "Patient")]
public class PaymentController : Controller
{
    private readonly AppointmentApiClient _appointmentApiClient;

    public PaymentController(AppointmentApiClient appointmentApiClient)
    {
        _appointmentApiClient = appointmentApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Success(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        if (appointmentId <= 0)
            return RedirectToAction("Dashboard", "Patient");

        try
        {
            var appointment = await _appointmentApiClient
                .GetMyAppointmentAsync(appointmentId, cancellationToken);

            if (appointment is null)
                return NotFound();

            return View(appointment);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth");
        }
        catch (HttpRequestException)
        {
            return RedirectToAction("Dashboard", "Patient");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Cancel(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        if (appointmentId > 0)
        {
            try
            {
                await _appointmentApiClient
                    .CancelPendingAppointmentAsync(appointmentId, cancellationToken);
            }
            catch (Exception)
            {
                
            }
        }

        return View();
    }
}