using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Chat
{
    public interface IChatRealtimeService
    {
        Task SendMessageAsync(
            string receiverUserId,
            object message);

        Task SendReadReceiptAsync(
            string receiverUserId,
            object readReceipt);
    }
}
