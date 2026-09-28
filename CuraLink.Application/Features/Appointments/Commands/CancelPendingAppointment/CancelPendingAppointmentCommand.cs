using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Commands.CancelPendingAppointment
{
    public record CancelPendingAppointmentCommand(int AppointmentId, string ApplicationUserId)
    : IRequest<bool>;
}
