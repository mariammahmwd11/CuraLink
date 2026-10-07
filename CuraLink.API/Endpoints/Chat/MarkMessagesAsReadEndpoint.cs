using CuraLink.Application.Features.Chat.Commands.MarkMessagesAsRead;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Chat
{
    public static class MarkMessagesAsReadEndpoint
    {
        public static void MapMarkMessagesAsReadEndpoint(
            this WebApplication app)
        {
            app.MapPost(
                "/api/chat/{appointmentId:int}/read",
                async (
                    int appointmentId,
                    ClaimsPrincipal user,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = user.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await sender.Send(
                        new MarkMessagesAsReadCommand(
                            appointmentId,
                            userId),
                        cancellationToken);

                    return Results.Ok(result);
                })
                .RequireAuthorization().WithTags("Chat");
        }
    }
}