using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointment
{
    public record GetMyAppointmentQuery(int AppointmentId, string ApplicationUserId)
    : IRequest<AppointmentDetailsDto?>;
}
