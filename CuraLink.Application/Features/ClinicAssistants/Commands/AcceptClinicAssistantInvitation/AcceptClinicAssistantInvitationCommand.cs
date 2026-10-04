using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.AcceptClinicAssistantInvitation
{
    public record AcceptClinicAssistantInvitationCommand(
     string UserId,
     string Token
 ) : IRequest;
}
