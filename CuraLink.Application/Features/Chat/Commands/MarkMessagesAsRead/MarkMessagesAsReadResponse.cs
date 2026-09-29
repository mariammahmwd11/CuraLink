using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.MarkMessagesAsRead
{
    public record MarkMessagesAsReadResponse(
     int AppointmentId,
     int MessagesMarkedAsRead,
     DateTime ReadAt
 );
}
