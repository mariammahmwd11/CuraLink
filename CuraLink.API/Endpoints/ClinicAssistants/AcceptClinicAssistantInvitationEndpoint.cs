using System.Security.Claims;
using MediatR;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.AcceptClinicAssistantInvitation;

public static class AcceptClinicAssistantInvitationEndpoint
{
    public static void MapAcceptClinicAssistantInvitationEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/clinic-assistants/invitations/accept",
            async (
                AcceptClinicAssistantInvitationRequest request,
                HttpContext httpContext,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId =
                    httpContext.User.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                await sender.Send(
                    new AcceptClinicAssistantInvitationCommand(
                        userId,
                        request.Token),
                    cancellationToken);

                return Results.Ok(new
                {
                    message =
                        "Invitation accepted successfully."
                });
            })
            .RequireAuthorization()
            .WithTags("Clinic Assistants");
    }

    public record AcceptClinicAssistantInvitationRequest(
        string Token);
}