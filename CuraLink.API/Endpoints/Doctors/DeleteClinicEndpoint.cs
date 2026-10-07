using System.Security.Claims;
using CuraLink.Application.Features.Clinics.Commands.DeleteClinic;
using MediatR;

namespace CuraLink.API.Endpoints.Doctors
{
    public static class DeleteClinicEndpoint
    {
        public static void MapDeleteClinicEndpoint(this WebApplication app)
        {
            app.MapDelete(
                "/api/doctor/clinics/{id:guid}",
                async (
                    Guid id,
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

                    var command = new DeleteClinicCommand
                    {
                        Id = id,
                        UserId = userId
                    };

                    await sender.Send(
                        command,
                        cancellationToken);

                    return Results.NoContent();
                })
                .RequireAuthorization("Doctor").WithTags("Clinics");
        }
    }
}