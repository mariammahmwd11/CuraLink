using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using MediatR;

namespace CuraLink.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords;

public class GetPatientMedicalDocsHandler
    : IRequestHandler<
        GetPatientMedicalDocsQuery,
        List<MedicalDocumentDto>>
{
    private readonly IMedicalDocumentRepository _medicalDocumentRepository;

    public GetPatientMedicalDocsHandler(
        IMedicalDocumentRepository  medicalDocumentRepository)
    {
        _medicalDocumentRepository = medicalDocumentRepository;
    }

    public async Task<List<MedicalDocumentDto>> Handle(
        GetPatientMedicalDocsQuery request,
        CancellationToken cancellationToken)
    {
        return await _medicalDocumentRepository.GetPatientDocumentsAsync(
            request.PatientId,
            cancellationToken);
    }
}