using System.Security.Claims;
using CuraLink.Application.Features.Appointments.Queries.GetMyAppointments;
using MediatR;

namespace CuraLink.API.Endpoints.Appointments;

public static class GetMyAppointmentsEndpoint
{
    public static void MapGetMyAppointmentsEndpoint(this WebApplication app)
    {
        app.MapGet(
            "/api/appointments/my",
            async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId = user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetMyAppointmentsQuery(applicationUserId),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization();
    }
}