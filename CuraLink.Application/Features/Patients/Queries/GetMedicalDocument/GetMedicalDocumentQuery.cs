using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using MediatR;

namespace CuraLink.Application.Features.Patients.Queries.GetMedicalDocument;

public class GetMedicalDocumentQuery : IRequest<MedicalDocumentDto?>
{
    public string UserId { get; set; } = null!;

    public int DocumentId { get; set; }
}