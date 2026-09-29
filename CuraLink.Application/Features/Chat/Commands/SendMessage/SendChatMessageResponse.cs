using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.SendMessage
{
    public record SendChatMessageResponse(
    int MessageId,
    int AppointmentId,
    string SenderId,
    string ReceiverId,
    string Content,
    DateTime SentAt,
    bool IsRead
);
}
