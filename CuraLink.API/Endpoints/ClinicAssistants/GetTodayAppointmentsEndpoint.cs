using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Queries.GetTodayAppointments;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class GetTodayAppointmentsEndpoint
{
    public static void MapGetTodayAppointmentsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinic-assistants/appointments/today",
            async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetTodayAppointmentsQuery(
                        applicationUserId),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization("Receptionist");
    }
}