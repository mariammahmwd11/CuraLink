using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantInvitation
{
    public record GetClinicAssistantInvitationResponse(
    string Email,
    bool IsRegistered,
    string ClinicName
);
}
