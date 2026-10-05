using CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantInvitation;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class GetClinicAssistantInvitationEndpoint
{
    public static void MapGetClinicAssistantInvitationEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinic-assistants/invitations",
            async (
                string token,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetClinicAssistantInvitationQuery(token),
                    cancellationToken);

                return Results.Ok(result);
            })
            .AllowAnonymous()
            .WithTags("Clinic Assistants");
    }
}