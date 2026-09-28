using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointment
{
    public class AppointmentDetailsDto
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string AppointmentStatus { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public decimal ConsultationPrice { get; set; }
    }
}
