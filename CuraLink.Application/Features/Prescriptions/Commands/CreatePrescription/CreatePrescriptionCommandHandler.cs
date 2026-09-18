using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Prescriptions;
using MediatR;

namespace CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandHandler
    : IRequestHandler<CreatePrescriptionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePrescriptionCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreatePrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get doctor
        var doctor = await _unitOfWork.Doctors
            .GetByApplicationUserIdAsync(
                request.UserId,
                cancellationToken);

        if (doctor == null)
        {
            throw new KeyNotFoundException(
                "Doctor not found.");
        }

        // 2. Doctor must be verified
        if (doctor.Status != DoctorStatusEnum.verified)
        {
            throw new InvalidOperationException(
                "Only verified doctors can create prescriptions.");
        }

        // 3. Get patient
        var patient = await _unitOfWork.Patients
     .GetByApplicationUserIdAsync(
         request.PatientUserId,
         cancellationToken);

        if (patient == null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        // 4. Create prescription
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),

            PatientId = patient.Id,

            DoctorId = doctor.Id,

            StartDate = request.StartDate,

            EndDate = request.EndDate,

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        // 5. Add prescription items
        foreach (var itemRequest in request.Items)
        {
            var item = new PrescriptionItem
            {
                Id = Guid.NewGuid(),

                MedicationName = itemRequest.MedicationName,

                Dosage = itemRequest.Dosage,

                Instructions = itemRequest.Instructions
            };

            // 6. Add dosage schedules
            foreach (var dosageTime in itemRequest.DosageTimes)
            {
                item.Schedules.Add(
                    new DosageSchedule
                    {
                        Id = Guid.NewGuid(),

                        DosageTime = dosageTime
                    });
            }

            prescription.Items.Add(item);
        }

        // 7. Add prescription
        await _unitOfWork.Prescriptions
            .AddAsync(
                prescription,
                cancellationToken);

        // 8. Save changes
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return prescription.Id;
    }
}