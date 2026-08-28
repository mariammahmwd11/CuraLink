using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetPendingDoctors
{
    public class GetPendingDoctorsQuery:IRequest<List<PendingDoctorDto>>
    {
    }
}
