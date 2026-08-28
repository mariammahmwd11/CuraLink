namespace CuraLink.MVC.Models.Admin
{

    public class PendingDoctorViewModel
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Specialty { get; set; } = string.Empty;

        public string SyndicateId { get; set; } = string.Empty;


        public int Status { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
