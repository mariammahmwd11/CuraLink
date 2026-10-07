using CuraLink.Application.Features.Chat.Commands.SendMessage;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Chat;

public static class SendChatMessageEndpoint
{
    public static void MapSendChatMessageEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/chat/{appointmentId:int}/messages",
            async (
                int appointmentId,
                SendChatMessageRequest request,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var command = new SendChatMessageCommand(
                    appointmentId,
                    request.Content,
                    userId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization().WithTags("Chat");
    }
}

public record SendChatMessageRequest(
    string Content);