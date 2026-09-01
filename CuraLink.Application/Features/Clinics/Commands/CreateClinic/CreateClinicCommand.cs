using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Clinics.Commands.CreateClinic
{
    public class CreateClinicCommand : IRequest<Guid>
    {
        public string UserId { get; set; } = null!;

        public string ClinicName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public decimal ConsultationPrice { get; set; }

        public string PhoneNumber { get; set; } = null!;
    }
}
