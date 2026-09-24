
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Patients;

namespace CuraLink.Domain.Entities.Appointments;

public class Appointment
{
    public int Id { get; set; }

    public Guid DoctorId { get; set; }

    public Guid PatientId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public AppointmentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public Patient Patient { get; set; } = null!;
}

