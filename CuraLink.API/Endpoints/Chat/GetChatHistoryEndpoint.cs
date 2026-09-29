using CuraLink.Application.Features.Chat.Queries.GetChatHistory;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Chat
{
    public static class GetChatHistoryEndpoint
    {
        public static void MapGetChatHistoryEndpoint(
            this WebApplication app)
        {
            app.MapGet(
                "/api/chat/{appointmentId:int}/messages",
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
                        new GetChatHistoryQuery(
                            appointmentId,
                            userId),
                        cancellationToken);

                    return Results.Ok(result);
                })
                .RequireAuthorization();
        }
    }
}