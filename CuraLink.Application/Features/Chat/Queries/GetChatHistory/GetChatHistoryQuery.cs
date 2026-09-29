using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Queries.GetChatHistory
{
    public record GetChatHistoryQuery(
     int AppointmentId,
     string UserId
 ) : IRequest<List<ChatMessageDto>>;
}
