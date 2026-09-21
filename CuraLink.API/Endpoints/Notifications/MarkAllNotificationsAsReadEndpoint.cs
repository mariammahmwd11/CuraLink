using CuraLink.Application.Features.Notifications.Commands.MarkAllAsRead;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    public static class MarkAllNotificationsAsReadEndpoint
    {
        public static void MapMarkAllNotificationsAsReadEndpoint(this WebApplication app)
        {
            app.MapPatch(
                "/api/notifications/read-all",
                async (
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
                        new MarkAllNotificationsAsReadCommand(userId),
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
