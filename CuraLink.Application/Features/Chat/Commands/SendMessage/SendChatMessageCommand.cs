using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.SendMessage
{
    public record SendChatMessageCommand(
    int AppointmentId,
    string Content,
    string SenderUserId
) : IRequest<SendChatMessageResponse>;
}
