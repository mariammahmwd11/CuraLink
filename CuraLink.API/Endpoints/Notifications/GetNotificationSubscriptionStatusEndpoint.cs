using CuraLink.Application.Features.Notifications.Queries.GetSubscriptionStatus;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    // Lets the MVC Dashboard know, server-side, whether the patient already
    // has an active Web Push subscription — so it can decide whether to show
    // the "Enable Medication Reminders" card or the notification bell,
    // without relying on the browser's Notification.permission alone.
    public static class GetNotificationSubscriptionStatusEndpoint
    {
        public static void MapGetNotificationSubscriptionStatusEndpoint(this WebApplication app)
        {
            app.MapGet(
                "/api/notifications/subscription-status",
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

                    var subscribed = await sender.Send(
                        new GetNotificationSubscriptionStatusQuery(userId),
                        cancellationToken);

                    return Results.Ok(new { subscribed });
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Patient"))
                .WithTags("Notifications");
        }
    }
}
