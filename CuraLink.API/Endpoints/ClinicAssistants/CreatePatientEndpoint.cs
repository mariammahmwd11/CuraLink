using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Commands.CreatePatient;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class CreatePatientEndpoint
{
    public static void MapCreatePatientEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/clinic-assistants/patients",
            async (
                CreatePatientCommand command,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var request = command with
                {
                    ApplicationUserId = applicationUserId
                };

                var patientId = await sender.Send(
                    request,
                    cancellationToken);

                return Results.Ok(new
                {
                    patientId,
                    message = "Patient created successfully."
                });
            })
        .RequireAuthorization("Receptionist").WithTags("Clinic Assistants");
    }
}