using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Clinics.Commands.DeleteClinic
{
    public class DeleteClinicCommandHandler
        : IRequestHandler<DeleteClinicCommand>
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly IClinicRepository clinicRepository;
        private readonly IUnitOfWork unitOfWork;

        public DeleteClinicCommandHandler(
            IDoctorRepository doctorRepository,
            IClinicRepository clinicRepository,
            IUnitOfWork unitOfWork)
        {
            this.doctorRepository = doctorRepository;
            this.clinicRepository = clinicRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handle(
            DeleteClinicCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Get doctor from logged-in user
            var doctor = await doctorRepository
                .GetByApplicationUserIdAsync(
                    request.UserId,
                    cancellationToken);

            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            // 2. Get clinic
            var clinic = await clinicRepository
                .GetByIdAsync(
                    request.Id,
                    cancellationToken);

            if (clinic == null)
            {
                throw new KeyNotFoundException("Clinic not found.");
            }

            // 3. Make sure clinic belongs to this doctor
            if (clinic.DoctorId != doctor.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to delete this clinic.");
            }

            // 4. Delete clinic
            clinicRepository.Delete(clinic);

            // 5. Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}