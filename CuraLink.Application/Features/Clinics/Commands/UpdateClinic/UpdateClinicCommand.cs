using MediatR;

namespace CuraLink.Application.Features.Clinics.Commands.UpdateClinic
{
    public class UpdateClinicCommand : IRequest
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        public string ClinicName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public decimal ConsultationPrice { get; set; }

        public string PhoneNumber { get; set; } = null!;
    }
}