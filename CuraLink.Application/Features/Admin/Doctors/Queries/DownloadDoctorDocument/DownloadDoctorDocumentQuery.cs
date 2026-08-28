using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.DownloadDoctorDocument
{
    public record DownloadDoctorDocumentQuery(
        Guid DoctorId,
        int DocumentId
    ) : IRequest<DownloadDoctorDocumentResult>;
}
