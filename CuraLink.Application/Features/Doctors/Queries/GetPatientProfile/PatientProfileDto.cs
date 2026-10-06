namespace CuraLink.Application.Features.Doctors.Queries.GetPatientProfile;

public class PatientProfileDto
{
    public Guid PatientId { get; set; }

    public string PatientUserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int Age { get; set; }

    public string? BloodType { get; set; }

    public MedicalHistoryDto? MedicalHistory { get; set; }

    public List<MedicalDocumentDto> Documents { get; set; }
        = new();
}

public class MedicalHistoryDto
{
    public Guid Id { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class MedicalDocumentDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }
}