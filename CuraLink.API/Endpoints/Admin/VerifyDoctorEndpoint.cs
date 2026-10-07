using CuraLink.Application.Features.Admin.Doctors.Commands.VerifyDoctor;
using MediatR;

namespace CuraLink.API.Endpoints.Admin
{
    public static class VerifyDoctorEndpoint
    {
        public static void MapVerifyDoctorEndpoint(
            this WebApplication app)
        {
            app.MapPut(
                "/api/admin/verify-doctor/{id:guid}",
                async (
                    Guid id,
                    VerifyDoctorCommand command,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    
                    command.DoctorId = id;

                    await sender.Send(
                        command,
                        cancellationToken);

                    return Results.Ok(new
                    {
                        message = command.IsApproved
                            ? "Doctor verified successfully."
                            : "Doctor rejected successfully."
                    });
                })
                .RequireAuthorization("AdminOnly")
                .WithName("VerifyDoctor")
                .WithTags("Admin")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound);
        }
    }
}