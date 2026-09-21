using CuraLink.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    public static class GetMyNotificationsEndpoint
    {
        public static void MapGetMyNotificationsEndpoint(this WebApplication app)
        {
            app.MapGet(
                "/api/notifications",
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

                    var result = await sender.Send(
                        new GetMyNotificationsQuery(userId),
                        cancellationToken);

                    return Results.Ok(result);
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Patient"))
                .WithTags("Notifications");
        }
    }
}
