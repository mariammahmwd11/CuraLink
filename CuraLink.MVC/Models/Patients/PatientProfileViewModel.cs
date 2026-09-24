using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Patients
{
    public class PatientProfileViewModel
    {
      
        public string? UserId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        
        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        public bool HasName =>
            !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName);
    }
}