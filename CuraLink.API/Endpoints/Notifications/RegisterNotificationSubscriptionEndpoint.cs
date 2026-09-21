using CuraLink.Application.Features.Notifications.Commands.RegisterSubscription;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CuraLink.API.Endpoints.Notifications
{
    public static class RegisterNotificationSubscriptionEndpoint
    {
        public static void MapRegisterNotificationSubscriptionEndpoint(
            this WebApplication app)
        {
            app.MapPost(
                "/api/notifications/subscription",
                async (
                    [FromBody]
                RegisterNotificationSubscriptionRequest request,
                    HttpContext httpContext,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var userId = httpContext.User
                        .FindFirstValue("UserId");

                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }

                    await sender.Send(
                        new RegisterNotificationSubscriptionCommand(
                            userId,
                            request.Endpoint,
                            request.P256DH,
                            request.Auth),
                        cancellationToken);

                    return Results.Ok(new
                    {
                        Message =
                            "Notification subscription registered successfully."
                    });
                })
                .RequireAuthorization(policy =>
                    policy.RequireRole("Patient"))
                .WithTags("Notifications");
        }
    }
}
