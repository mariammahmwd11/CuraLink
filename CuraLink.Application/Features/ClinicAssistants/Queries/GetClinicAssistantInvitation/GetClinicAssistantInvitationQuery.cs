using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantInvitation
{
    public record GetClinicAssistantInvitationQuery(
     string Token
 ) : IRequest<GetClinicAssistantInvitationResponse>;
}
