using System.Security.Claims;
using CuraLink.Application.Features.Doctors.Queries.GetDoctorSchedule;
using MediatR;

namespace CuraLink.API.Endpoints.Doctors;

public static class GetDoctorScheduleEndpoint
{
    public static void MapGetDoctorScheduleEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/doctor/schedule",
            async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetDoctorScheduleQuery(
                        applicationUserId),
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization("Doctor").WithTags("Doctors");
    }
}