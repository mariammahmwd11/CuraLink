using System.Security.Claims;
using CuraLink.Application.Features.Clinics.Commands.UpdateClinic;
using MediatR;

namespace CuraLink.API.Endpoints.Doctors
{
    public static class UpdateClinicEndpoint
    {
        public static void MapUpdateClinicEndpoint(this WebApplication app)
        {
            app.MapPut(
                "/api/doctor/clinics/{id:guid}",
                async (
                    Guid id,
                    UpdateClinicCommand command,
                    ClaimsPrincipal user,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId =
                        user.FindFirst("UserId")?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    command.Id = id;
                    command.UserId = userId;

                    await sender.Send(
                        command,
                        cancellationToken);

                    return Results.NoContent();
                })
                .RequireAuthorization("Doctor").WithTags("Clinics");
        }
    }
}