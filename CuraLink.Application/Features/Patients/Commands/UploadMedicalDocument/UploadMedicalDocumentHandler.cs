using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.MedicalHistories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Patients.Commands.UploadMedicalDocument
{
    public class UploadMedicalDocumentHandler
    : IRequestHandler<UploadMedicalDocumentCommand, int>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public UploadMedicalDocumentHandler(IUnitOfWork unitOfWork,
          
            IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<int> Handle(
             UploadMedicalDocumentCommand request,
             CancellationToken cancellationToken)
        {
            var patient =
                await unitOfWork.Patients.GetByApplicationUserIdAsync(
                    request.UserId,
                    cancellationToken);

            if (patient == null)
            {
                throw new InvalidOperationException(
                    "Patient not found.");
            }

            var medicalHistory =
                await unitOfWork.MedicalHistories.GetByPatientIdAsync(
                    patient.Id,
                    cancellationToken);

            if (medicalHistory == null)
            {
                throw new InvalidOperationException(
                    "Medical history not found.");
            }

            var file = request.File;

            using var stream = file.OpenReadStream();

            var storageKey = await _fileStorageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                "patients/medical-records",
                cancellationToken);

            var document = new MedicalDocument
            {
                MedicalHistoryId = medicalHistory.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                StorageKey = storageKey,
                UploadedAt = DateTime.UtcNow
            };

            await unitOfWork.MedicalDocuments.AddAsync(
                document,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return document.Id;
        }
    }
}