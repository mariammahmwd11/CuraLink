using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Doctors.Queries.GetMyPatients
{
    public record GetMyPatientsQuery(
     string UserId
 ) : IRequest<List<PatientListDto>>;
}
