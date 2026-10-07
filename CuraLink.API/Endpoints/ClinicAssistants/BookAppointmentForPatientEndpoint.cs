using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Commands.BookAppointmentForPatient;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class BookAppointmentForPatientEndpoint
{
    public static void MapBookAppointmentForPatientEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/clinic-assistants/appointments",
            async (
                BookAppointmentForPatientCommand command,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var request = command with
                {
                    ApplicationUserId = applicationUserId
                };

                var appointmentId = await sender.Send(
                    request,
                    cancellationToken);

                return Results.Ok(new
                {
                    appointmentId,
                    message = "Appointment booked successfully."
                });
            })
        .RequireAuthorization("Receptionist").WithTags("Clinic Assistants");
    }
}