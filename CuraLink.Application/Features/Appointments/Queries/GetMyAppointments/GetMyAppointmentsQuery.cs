using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointments
{
    public record GetMyAppointmentsQuery(string ApplicationUserId)
     : IRequest<List<MyAppointmentListItemDto>>;
}
