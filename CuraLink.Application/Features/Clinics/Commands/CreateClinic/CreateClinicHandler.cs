using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Clinics.Commands.CreateClinic
{
    public class CreateClinicCommandHandler
        : IRequestHandler<CreateClinicCommand, Guid>
    {
       
        private readonly IDoctorRepository doctorRepository;
        private readonly IClinicRepository clinicRepository;
        private readonly IUnitOfWork unitOfWork;

        public CreateClinicCommandHandler(IDoctorRepository doctorRepository,IClinicRepository clinicRepository,IUnitOfWork unitOfWork)
        {
            
            this.doctorRepository = doctorRepository;
            this.clinicRepository = clinicRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(
            CreateClinicCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Get doctor
            var doctor = await doctorRepository
      .GetByApplicationUserIdAsync(
          request.UserId,
          cancellationToken);


            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            // 3. Make sure doctor is verified
            if (doctor.Status != DoctorStatusEnum.verified)
            {
                throw new InvalidOperationException(
                    "Only verified doctors can create a clinic.");
            }

          
            var clinic = new Clinic
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                ClinicName = request.ClinicName,
                Address = request.Address,
                ConsultationPrice = request.ConsultationPrice,
                PhoneNumber = request.PhoneNumber
            };

           
            await clinicRepository.AddAsync(
                clinic,
                cancellationToken);

            // 6. Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return clinic.Id;
        }
    }
}