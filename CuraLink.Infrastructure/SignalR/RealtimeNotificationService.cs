

using CuraLink.Application.Common.Interfaces;
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace CuraLink.API.Infrastructure.SignalR;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealtimeNotificationService(
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(
        string userId,
        string title,
        string message)
    {
        await _hubContext.Clients
            .User(userId)
            .SendAsync(
                "ReceiveNotification",
                new
                {
                    title,
                    message
                });
    }
}