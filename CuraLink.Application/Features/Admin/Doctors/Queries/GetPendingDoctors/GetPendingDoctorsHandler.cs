using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Queries.GetPendingDoctors
{
    public class GetPendingDoctorsHandler
    : IRequestHandler<GetPendingDoctorsQuery, List<PendingDoctorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public GetPendingDoctorsHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<PendingDoctorDto>> Handle(
    GetPendingDoctorsQuery request,
    CancellationToken cancellationToken)
        {
            var doctors = await _unitOfWork.Doctors
                .GetPendingDoctorsAsync(cancellationToken);

            var result = new List<PendingDoctorDto>();

            foreach (var doctor in doctors)
            {
                var user = await _unitOfWork.Users
                    .GetByIdAsync(
                        doctor.ApplicationUserId,
                        cancellationToken);

                if (user is null)
                    continue;

                result.Add(new PendingDoctorDto
                {
                    Id = doctor.Id,

                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    Specialty = doctor.Specialty,
                    SyndicateId = doctor.SyndicateId,
                    Status = doctor.Status,

                   
                });
            }

            return result;
        }
    }
    }