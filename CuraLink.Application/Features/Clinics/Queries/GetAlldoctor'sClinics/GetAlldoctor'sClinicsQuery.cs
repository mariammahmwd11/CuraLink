using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics
{
    public record GetAlldoctor_sClinicsQuery(Guid DoctorId)
     : IRequest<IEnumerable<ClinicDto>>;
}
