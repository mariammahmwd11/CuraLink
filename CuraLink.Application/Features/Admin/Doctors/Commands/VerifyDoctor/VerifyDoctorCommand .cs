using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Commands.VerifyDoctor
{
    public class VerifyDoctorCommand : IRequest
    {
        public Guid DoctorId { get; set; }

        public bool IsApproved { get; set; }

        public string? RejectionReason { get; set; }
    }
}
