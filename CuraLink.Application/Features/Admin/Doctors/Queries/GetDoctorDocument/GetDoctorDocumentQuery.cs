using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetDoctorDocument
{
    public class GetDoctorDocumentQuery : IRequest<List<DoctorDocumentResult>>
    {
        public Guid DoctorId { get; set; }

        public GetDoctorDocumentQuery(Guid doctorId)
        {
            DoctorId = doctorId;
        }
    }
}
