using CuraLink.Application.Common.Exceptions;
using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetDoctorDocument
{
    public class GetDoctorDocumentHandler
        : IRequestHandler<GetDoctorDocumentQuery, List<DoctorDocumentResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public GetDoctorDocumentHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<DoctorDocumentResult>> Handle(
            GetDoctorDocumentQuery request,
            CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(
                request.DoctorId,
                cancellationToken);

            if (doctor is null)
                throw new NotFoundException(
                    nameof(Doctor),
                    request.DoctorId);

            var documents =
                await _unitOfWork.DoctorDocuments.GetAllByDoctorIdAsync(
                    request.DoctorId,
                    cancellationToken);

            var result = new List<DoctorDocumentResult>();

            foreach (var document in documents)
            {
                var url = await _fileStorageService.GetUrlAsync(
                    document.StorageKey,
                    cancellationToken);

                result.Add(new DoctorDocumentResult
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    Url = url
                });
            }

            return result;
        }
    }
}