using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models
{
    public class InvitationViewModel
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        public string ClinicName { get; set; } = "";
        public bool IsRegistered { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RegisterInvitationViewModel
    {
        [Required]
        public string Token { get; set; } = "";

        // Display only. The email is NOT sent to the API; the backend derives it from the token.
        public string Email { get; set; } = "";
        public string ClinicName { get; set; } = "";

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Enter a valid phone number (digits only, 8-15 digits).")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";

        public string? ErrorMessage { get; set; }
    }

    public class InvitationAcceptedViewModel
    {
        public string ClinicName { get; set; } = "";
    }
}