using CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistants;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class GetClinicAssistantsEndpoint
{
    public static void MapGetClinicAssistantsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinics/{clinicId:guid}/assistants",
            async (
                Guid clinicId,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var doctorUserId =
                    user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(doctorUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetClinicAssistantsQuery(
                        doctorUserId,
                        clinicId),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization("Doctor")
        .WithTags("Clinic Assistants");
    }
}