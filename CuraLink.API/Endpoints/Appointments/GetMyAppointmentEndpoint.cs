using System.Security.Claims;
using CuraLink.Application.Features.Appointments.Queries.GetMyAppointment;
using MediatR;

namespace CuraLink.API.Endpoints.Appointments;

public static class GetMyAppointmentEndpoint
{
    public static void MapGetMyAppointmentEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/appointments/{id:int}",
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

                var result = await sender.Send(
                    new GetMyAppointmentQuery(id, applicationUserId),
                    cancellationToken);

                return result is null ? Results.NotFound() : Results.Ok(result);
            })
        .RequireAuthorization().WithTags("Appointments");
    }
}