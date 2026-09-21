using CuraLink.Application.Features.Notifications.Queries.GetUnreadCount;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    public static class GetUnreadNotificationCountEndpoint
    {
        public static void MapGetUnreadNotificationCountEndpoint(this WebApplication app)
        {
            app.MapGet(
                "/api/notifications/unread-count",
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

                    var count = await sender.Send(
                        new GetUnreadNotificationCountQuery(userId),
                        cancellationToken);

                    return Results.Ok(new { count });
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Patient"))
                .WithTags("Notifications");
        }
    }
}
