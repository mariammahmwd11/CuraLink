using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(
            string userId,
            string title,
            string message,
            CancellationToken cancellationToken = default);
    }
}
