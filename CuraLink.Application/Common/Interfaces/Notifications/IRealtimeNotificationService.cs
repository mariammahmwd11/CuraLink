using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Notifications
{
    public interface IRealtimeNotificationService
    {
        Task SendAsync(
            string userId,
            string title,
            string message);
    }
}
