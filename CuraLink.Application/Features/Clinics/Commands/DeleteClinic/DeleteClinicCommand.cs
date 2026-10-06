using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Clinics.Commands.DeleteClinic
{
    public class DeleteClinicCommand : IRequest
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;
    }
}
