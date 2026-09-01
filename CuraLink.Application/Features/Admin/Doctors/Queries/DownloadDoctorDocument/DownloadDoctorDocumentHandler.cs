using CuraLink.Application.Common.Exceptions;
using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.DownloadDoctorDocument
{
    public class DownloadDoctorDocumentHandler
        : IRequestHandler<
            DownloadDoctorDocumentQuery,
            DownloadDoctorDocumentResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public DownloadDoctorDocumentHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<DownloadDoctorDocumentResult> Handle(
            DownloadDoctorDocumentQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Check doctor exists
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(
                request.DoctorId,
                cancellationToken);

            if (doctor is null)
            {
                throw new NotFoundException(
                    nameof(Doctor),
                    request.DoctorId);
            }

            // 2. Get document
            var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(
                request.DocumentId,
                cancellationToken);

            if (document is null ||
                document.DoctorId != request.DoctorId)
            {
                throw new NotFoundException(
                    nameof(DoctorDocument),
                    request.DocumentId);
            }
            Console.WriteLine("========== DOWNLOAD DEBUG ==========");
            Console.WriteLine($"Request DoctorId: {request.DoctorId}");
            Console.WriteLine($"Request DocumentId: {request.DocumentId}");
            Console.WriteLine($"Document DoctorId: {document.DoctorId}");
            Console.WriteLine($"Document Id: {document.Id}");
            Console.WriteLine($"StorageKey: {document.StorageKey}");
            Console.WriteLine($"FileName: {document.FileName}");
            Console.WriteLine("====================================");

            // 3. Download from Cloudinary
            var fileStream = await _fileStorageService.DownloadAsync(
                document.StorageKey,
                cancellationToken);

            // 4. Return file information
            return new DownloadDoctorDocumentResult
            {
                FileStream = fileStream,
                FileName = document.FileName,
                ContentType = document.ContentType
            };
        }
    }
}