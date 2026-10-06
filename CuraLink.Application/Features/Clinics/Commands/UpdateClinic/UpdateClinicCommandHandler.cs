using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Clinics.Commands.UpdateClinic
{
    public class UpdateClinicCommandHandler
        : IRequestHandler<UpdateClinicCommand>
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly IClinicRepository clinicRepository;
        private readonly IUnitOfWork unitOfWork;

        public UpdateClinicCommandHandler(
            IDoctorRepository doctorRepository,
            IClinicRepository clinicRepository,
            IUnitOfWork unitOfWork)
        {
            this.doctorRepository = doctorRepository;
            this.clinicRepository = clinicRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handle(
            UpdateClinicCommand request,
            CancellationToken cancellationToken)
        {
            var doctor = await doctorRepository
                .GetByApplicationUserIdAsync(
                    request.UserId,
                    cancellationToken);

            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            var clinic = await clinicRepository
                .GetByIdAsync(
                    request.Id,
                    cancellationToken);

            if (clinic == null)
            {
                throw new KeyNotFoundException("Clinic not found.");
            }

            if (clinic.DoctorId != doctor.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to update this clinic.");
            }

            clinic.ClinicName = request.ClinicName;
            clinic.Address = request.Address;
            clinic.ConsultationPrice = request.ConsultationPrice;
            clinic.PhoneNumber = request.PhoneNumber;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}