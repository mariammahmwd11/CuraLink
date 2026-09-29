using CuraLink.Application.Common.Interfaces.Chat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.SignalR
{
    public class ChatRealtimeService : IChatRealtimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public ChatRealtimeService(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageAsync(
            string receiverUserId,
            object message)
        {
            await _hubContext.Clients
                .User(receiverUserId)
                .SendAsync("ReceiveChatMessage", message);
        }

        public async Task SendReadReceiptAsync(
            string receiverUserId,
            object readReceipt)
        {
            await _hubContext.Clients
                .User(receiverUserId)
                .SendAsync("ChatMessageRead", readReceipt);
        }
    }
}
