using System.Security.Claims;
using CuraLink.Application.Common.Interfaces.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.API.Endpoints.Notifications;

public static class TestNotificationEndpoint
{
    public static void MapTestNotificationEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/notifications/test",
            async (
                [FromBody] TestNotificationRequest request,
                HttpContext httpContext,
                INotificationService notificationService,
                CancellationToken cancellationToken) =>
            {
                var userId = httpContext.User
                    .FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await notificationService.SendAsync(
                    userId,
                    request.Title,
                    request.Message,
                    cancellationToken);

                return Results.Ok(new
                {
                    Message = "Test notification sent successfully."
                });
            })
            .RequireAuthorization(policy =>
                policy.RequireRole("Patient"))
            .WithTags("Notifications");
    }
}

public record TestNotificationRequest(
    string Title,
    string Message);