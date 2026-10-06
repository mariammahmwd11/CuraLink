using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Commands.CheckInPatient;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class CheckInPatientEndpoint
{
    public static void MapCheckInPatientEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/clinic-assistants/appointments/{appointmentId:int}/check-in",
            async (
                int appointmentId,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                await sender.Send(
                    new CheckInPatientCommand(
                        applicationUserId,
                        appointmentId),
                    cancellationToken);

                return Results.Ok(new
                {
                    message = "Patient checked in successfully."
                });
            })
        .RequireAuthorization("Receptionist");
    }
}