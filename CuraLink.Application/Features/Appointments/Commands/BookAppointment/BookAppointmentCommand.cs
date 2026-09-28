using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Commands.BookAppointment
{
    public record BookAppointmentCommand(
        Guid DoctorId,
        Guid ClinicId,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string ApplicationUserId)
        : IRequest<int>;


}
