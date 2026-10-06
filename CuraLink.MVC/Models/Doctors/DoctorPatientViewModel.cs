namespace CuraLink.MVC.Models.Doctors;

public class DoctorPatientViewModel
{
    public Guid PatientId { get; set; }

    public Guid PatientUserId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public int Age { get; set; }

    public string? BloodType { get; set; }
}