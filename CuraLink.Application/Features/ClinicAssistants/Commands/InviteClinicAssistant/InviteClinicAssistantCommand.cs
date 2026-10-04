using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.InviteClinicAssistant
{
    public record InviteClinicAssistantCommand(
     string DoctorUserId,
    Guid ClinicId,
    string Email
) : IRequest;
}
