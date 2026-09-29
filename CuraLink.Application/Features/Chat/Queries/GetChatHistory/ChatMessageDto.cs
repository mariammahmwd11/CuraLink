using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Queries.GetChatHistory
{
    public record ChatMessageDto(
     int Id,
     int AppointmentId,
     string SenderId,
     string ReceiverId,
     string Content,
     DateTime SentAt,
     bool IsRead,
     DateTime? ReadAt
 );
}
