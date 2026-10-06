using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetTodayAppointments
{
    public record GetTodayAppointmentsQuery(
    string ApplicationUserId
) : IRequest<IReadOnlyList<TodayAppointmentDto>>;
}
