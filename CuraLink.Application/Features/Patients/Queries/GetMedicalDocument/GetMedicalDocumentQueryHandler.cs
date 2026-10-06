using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using MediatR;

namespace CuraLink.Application.Features.Patients.Queries.GetMedicalDocument;

public class GetMedicalDocumentHandler
    : IRequestHandler<GetMedicalDocumentQuery, MedicalDocumentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicalDocumentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MedicalDocumentDto?> Handle(
        GetMedicalDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _unitOfWork.Patients.GetByApplicationUserIdAsync(
                request.UserId,
                cancellationToken);

        if (patient == null)
        {
            return null;
        }

        var document =
            await _unitOfWork.MedicalDocuments.GetByIdForPatientAsync(
                request.DocumentId,
                patient.Id,
                cancellationToken);

        if (document == null)
        {
            return null;
        }

        return new MedicalDocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            StorageKey = document.StorageKey,
            UploadedAt = document.UploadedAt
        };
    }
}