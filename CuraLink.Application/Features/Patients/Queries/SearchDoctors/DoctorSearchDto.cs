namespace CuraLink.Application.Features.Patients.Queries.SearchDoctors
{
    public class DoctorSearchDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public string FullName { get; set; } = null!;

        public string Specialty { get; set; } = null!;
        public string? ProfilePhoto { get; set; }
        public string Address { get; set; } = null!;

        public string Governorate { get; set; } = null!;

        public decimal ConsultationPrice { get; set; }

        public double Rating { get; set; }

        public List<DateTime> AvailableDates { get; set; }
            = new();
    }
}