using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Chat.Commands.MarkMessagesAsRead
{
    public record MarkMessagesAsReadCommand(
    int AppointmentId,
    string UserId
) : IRequest<MarkMessagesAsReadResponse>;
}
