using CuraLink.Application.Features.ClinicAssistants.Commands.InviteClinicAssistant;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.ClinicAssistants
{
    public static class InviteClinicAssistantEndpoint
    {
        public static void MapInviteClinicAssistantEndpoint(
            this WebApplication app)
        {
            app.MapPost(
                "/api/clinics/{clinicId:guid}/assistants/invite",
                async (
                    Guid clinicId,
                    InviteClinicAssistantRequest request,
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User
                        .FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    await sender.Send(
                        new InviteClinicAssistantCommand(
                            userId,
                            clinicId,
                            request.Email),
                        cancellationToken);

                    return Results.Ok(new
                    {
                        message = "Assistant invitation sent successfully."
                    });
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Doctor"))
                .WithTags("Clinic Assistants");
        }

        public record InviteClinicAssistantRequest(string Email);
    }
}
