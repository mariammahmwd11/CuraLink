using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Doctors;

public record AssistantClinicOption(Guid Id, string Name);

public class ClinicAssistantViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class DoctorAssistantsViewModel
{
    public List<AssistantClinicOption> Clinics { get; set; } = new();
    public Guid SelectedClinicId { get; set; }
    public string SelectedClinicName { get; set; } = "";
    public List<ClinicAssistantViewModel> Assistants { get; set; } = new();
}

public class AddAssistantViewModel
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = "";
}