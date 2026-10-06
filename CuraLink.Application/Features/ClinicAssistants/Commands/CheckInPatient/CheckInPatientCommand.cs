using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.CheckInPatient
{
    public record CheckInPatientCommand(
      string ApplicationUserId,
      int AppointmentId
  ) : IRequest;
}
