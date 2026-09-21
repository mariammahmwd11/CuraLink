namespace CuraLink.MVC.Models.Prescriptions;

public class DoctorPrescriptionListItemViewModel
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Medications { get; set; } = new();

    public string MedicationsSummary =>
        Medications.Count == 0
            ? "—"
            : string.Join(", ", Medications);
}
