using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Patients;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterPatient
{
    public class RegisterPatientHandler : IRequestHandler<RegisterPatientCommand>
    {
        private readonly IIdentityService identityService;
        private readonly IPatientRepository patientRepository;
        private readonly IMedicalHistoryRepository medicalHistoryRepository;
        private readonly IUnitOfWork unitOfWork;

        public RegisterPatientHandler(IIdentityService identityService,IUnitOfWork unitOfWork)
        {
            this.identityService = identityService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {
            var result =await identityService.CreatePatientAsync(request.FirstName, request.LastName, request.Email, request.PhoneNumber,request.Password);
           
            if (!result.Succeeded)
            {
                throw new Exception(
                string.Join(", ", result.Errors));
            }
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = result.UserId
            };

            var medicalHistory = new MedicalHistory
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            patient.MedicalHistory = medicalHistory;
           await unitOfWork.Patients.AddAsync(patient);
           await unitOfWork.MedicalHistories.AddAsync(medicalHistory);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
