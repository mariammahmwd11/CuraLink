using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterDoctor
{
    public class RegisterDoctorHandler : IRequestHandler<RegisterDoctorCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IApplicationDbContext _context;

        public RegisterDoctorHandler(
            IIdentityService identityService,
            IFileStorageService fileStorageService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _context = context;
        }

        public async Task Handle(
            RegisterDoctorCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _identityService.CreateDoctorAsync(
           request.FirstName,
           request.LastName,
           request.Email,
           request.PhoneNumber,
           request.Password
           );

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(", ", result.Errors));
            }
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = result.UserId,
                Specialty = request.Specialty,
                SyndicateId = request.SyndicateId,
                Status = DoctorStatusEnum.PendingVerification
            };

            _context.Doctors.Add(doctor);


            using var stream = request.LicenseDocument.OpenReadStream();

            var storageKey = await _fileStorageService.UploadAsync(
                stream,
                request.LicenseDocument.FileName,
                request.LicenseDocument.ContentType,
                "doctors/licenses",
                cancellationToken);


            var document = new DoctorDocument
            {
                DoctorId = doctor.Id,
                FileName = request.LicenseDocument.FileName,
                ContentType = request.LicenseDocument.ContentType,
                FileSize = request.LicenseDocument.Length,
                StorageKey = storageKey,
                UploadedAt = DateTime.UtcNow
                
            };

            _context.DoctorDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        }
}
