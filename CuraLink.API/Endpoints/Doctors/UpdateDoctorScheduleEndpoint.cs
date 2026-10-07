using System.Security.Claims;
using CuraLink.Application.Features.Doctors.Commands.UpdateDoctorSchedule;
using MediatR;

namespace CuraLink.API.Endpoints.Doctors;

public static class UpdateDoctorScheduleEndpoint
{
    public static void MapUpdateDoctorScheduleEndpoint(
        this WebApplication app)
    {
        app.MapPut(
            "/api/doctor/schedule",
            async (
                UpdateDoctorScheduleRequest request,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
          user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                await sender.Send(
                    new UpdateDoctorScheduleCommand(
                        applicationUserId,
                        request.Availability),
                    cancellationToken);

                return Results.Ok(new
                {
                    message = "Doctor schedule updated successfully."
                });
            })
            .RequireAuthorization("Doctor").WithTags("Doctors");
    }
}