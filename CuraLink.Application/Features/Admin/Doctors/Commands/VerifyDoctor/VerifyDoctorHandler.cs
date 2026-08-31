using CuraLink.Application.Common.Exceptions;
using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Email;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctor;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Commands.VerifyDoctor
{

    public class VerifyDoctorHandler : IRequestHandler<VerifyDoctorCommand>
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;
        private readonly IIdentityService identityService;
        private readonly IEmailService emailService;

        public VerifyDoctorHandler(IDoctorRepository doctorRepository,IUnitOfWork unitOfWork, ICurrentUserService currentUserService,IIdentityService identityService,IEmailService emailService)

        { 
            this.doctorRepository = doctorRepository;
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
            this.identityService = identityService;
            this.emailService = emailService;
        }

        public async Task Handle(
    VerifyDoctorCommand request,
    CancellationToken cancellationToken)
        {
            var doctor = await doctorRepository.GetByIdAsync(
                request.DoctorId,
                cancellationToken);

            if (doctor == null)
                throw new NotFoundException(
                    $"Doctor with ID {request.DoctorId} was not found.");

            if (doctor.Status != DoctorStatusEnum.PendingVerification)
                throw new InvalidOperationException(
                    "Only pending doctors can be verified or rejected.");

            if (!Guid.TryParse(currentUserService.UserId, out var adminId))
                throw new UnauthorizedAccessException("Invalid admin user ID.");

            if (request.IsApproved)
            {
                doctor.Verify(adminId);

                await identityService.ActivateUserAsync(
                    doctor.ApplicationUserId);

                var userInfo = await identityService.GetUserInfoAsync(
                    doctor.ApplicationUserId);

                if (userInfo == null)
                    throw new NotFoundException(
                        "Doctor application user was not found.");

                await emailService.SendDoctorActivationEmailAsync(
                    userInfo.Value.Email,
                    $"{userInfo.Value.FirstName} {userInfo.Value.LastName}");
            }
            else
            {
                doctor.Reject(
                    request.RejectionReason!,
                    adminId);
                var userInfo = await identityService.GetUserInfoAsync(
        doctor.ApplicationUserId);

                if (userInfo == null)
                    throw new NotFoundException(
                        "Doctor application user was not found.");

                await emailService.SendDoctorRejectionEmailAsync(
                    userInfo.Value.Email,
                    $"{userInfo.Value.FirstName} {userInfo.Value.LastName}",
                    request.RejectionReason!);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

}
