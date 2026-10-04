using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.RegisterClinicAssistant
{
    public record RegisterClinicAssistantCommand(
      string Token,
      string FirstName,
      string LastName,
      string Phone,
      string Password
  ) : IRequest<string>;
}
