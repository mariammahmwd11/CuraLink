using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Patients.Commands.DeleteMedicalDocument;

public class DeleteMedicalDocumentHandler
    : IRequestHandler<DeleteMedicalDocumentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteMedicalDocumentHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<bool> Handle(
        DeleteMedicalDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _unitOfWork.Patients.GetByApplicationUserIdAsync(
                request.UserId,
                cancellationToken);

        if (patient == null)
        {
            return false;
        }

        var document =
            await _unitOfWork.MedicalDocuments.GetByIdForPatientAsync(
                request.DocumentId,
                patient.Id,
                cancellationToken);

        if (document == null)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync(
            document.StorageKey,
            "raw",
            cancellationToken);

        await _unitOfWork.MedicalDocuments.DeleteAsync(
            document,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}