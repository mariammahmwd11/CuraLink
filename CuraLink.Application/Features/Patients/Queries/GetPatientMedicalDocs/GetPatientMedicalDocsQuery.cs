using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs
{
    public record GetPatientMedicalDocsQuery(
    Guid PatientId
) : IRequest<List<MedicalDocumentDto>>;
}
