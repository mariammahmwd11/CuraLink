using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetTodayAppointments
{
    public record TodayAppointmentDto(
    int Id,
    string StartTime,
    string EndTime,
    string PatientName,
    string DoctorName,
    string Status
);
}
