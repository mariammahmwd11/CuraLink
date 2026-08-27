namespace CuraLink.MVC.Models.Auth
{
    /// <summary>
    /// View model for doctor registration form
    /// </summary>
    public class RegisterDoctorViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string SyndicateId { get; set; } = string.Empty;
        public IFormFile? LicenseDocument { get; set; }
    }
}
