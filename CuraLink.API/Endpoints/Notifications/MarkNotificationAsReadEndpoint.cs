using CuraLink.Application.Features.Notifications.Commands.MarkAsRead;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    public static class MarkNotificationAsReadEndpoint
    {
        public static void MapMarkNotificationAsReadEndpoint(this WebApplication app)
        {
            app.MapPatch(
                "/api/notifications/{id:guid}/read",
                async (
                    Guid id,
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User.FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var success = await sender.Send(
                        new MarkNotificationAsReadCommand(id, userId),
                        cancellationToken);

                    return success
                        ? Results.Ok()
                        : Results.NotFound();
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Patient"))
                .WithTags("Notifications");
        }
    }
}
