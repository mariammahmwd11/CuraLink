
using CuraLink.Domain.Entities.ChatMessages;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Patients;
using CuraLink.Domain.Entities.Payments;

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
    public Guid ClinicId { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Payment? Payment { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; }
    = new List<ChatMessage>();
}

