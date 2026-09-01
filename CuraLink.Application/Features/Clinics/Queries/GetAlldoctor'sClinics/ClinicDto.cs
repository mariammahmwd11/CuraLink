namespace CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics
{
    public class ClinicDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal ConsultationPrice { get; set; }

        public string? PhoneNumber { get; set; }
    }
}