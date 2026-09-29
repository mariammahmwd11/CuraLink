using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointments
{
    public record MyAppointmentListItemDto(
     int Id,
     string DoctorName,
     string PatientName,
     string? ClinicName,
     DateTime Date,
     TimeSpan StartTime,
     TimeSpan EndTime,
     string Status);
}
