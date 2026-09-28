namespace CuraLink.MVC.Models.Patients;

public class AppointmentDetailsViewModel
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public decimal ConsultationPrice { get; set; }

    public bool IsPaid =>
        string.Equals(AppointmentStatus, "Paid", StringComparison.OrdinalIgnoreCase);
}