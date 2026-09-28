using System.Security.Claims;
using CuraLink.Application.Features.Appointments.Commands.CancelPendingAppointment;
using MediatR;

namespace CuraLink.API.Endpoints.Appointments;

public static class CancelPendingAppointmentEndpoint
{
    public static void MapCancelPendingAppointmentEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/appointments/{id:int}/cancel-pending",
            async (
                int id,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var success = await sender.Send(
                    new CancelPendingAppointmentCommand(id, applicationUserId),
                    cancellationToken);

                return success ? Results.NoContent() : Results.NotFound();
            })
        .RequireAuthorization();
    }
}