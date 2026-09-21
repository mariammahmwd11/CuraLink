using CuraLink.Application.Common.Interfaces.BackgroundJobs;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.Prescriptions;
using MediatR;

namespace CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandHandler
    : IRequestHandler<CreatePrescriptionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDosageReminderScheduler _dosageReminderScheduler;

    public CreatePrescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IDosageReminderScheduler dosageReminderScheduler)
    {
        _unitOfWork = unitOfWork;
        _dosageReminderScheduler = dosageReminderScheduler;
    }

    public async Task<Guid> Handle(
        CreatePrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors
            .GetByApplicationUserIdAsync(
                request.UserId,
                cancellationToken);

        if (doctor == null)
        {
            throw new KeyNotFoundException(
                "Doctor not found.");
        }

        if (doctor.Status != DoctorStatusEnum.verified)
        {
            throw new InvalidOperationException(
                "Only verified doctors can create prescriptions.");
        }

        var patient = await _unitOfWork.Patients
            .GetByApplicationUserIdAsync(
                request.PatientUserId,
                cancellationToken);

        if (patient == null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        var doctorPatient = await _unitOfWork.DoctorPatients
            .GetAsync(
                doctor.Id,
                patient.Id,
                cancellationToken);

        if (doctorPatient == null)
        {
            doctorPatient = new DoctorPatient
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DoctorPatients
                .AddAsync(
                    doctorPatient,
                    cancellationToken);
        }

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

        foreach (var itemRequest in request.Items)
        {
            var item = new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescription.Id,
                MedicationName = itemRequest.MedicationName,
                Dosage = itemRequest.Dosage,
                Instructions = itemRequest.Instructions
            };

            foreach (var dosageTime in itemRequest.DosageTimes)
            {
                item.Schedules.Add(
                    new DosageSchedule
                    {
                        Id = Guid.NewGuid(),
                        PrescriptionItemId = item.Id,
                        DosageTime = dosageTime
                    });
            }

            prescription.Items.Add(item);
        }

        await _unitOfWork.Prescriptions
            .AddAsync(
                prescription,
                cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await ScheduleDosageRemindersAsync(
            prescription,
            cancellationToken);

        return prescription.Id;
    }

    private async Task ScheduleDosageRemindersAsync(
        Prescription prescription,
        CancellationToken cancellationToken)
    {
        var egyptTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                "Egypt Standard Time");

        for (
            var date = prescription.StartDate.Date;
            date <= prescription.EndDate.Date;
            date = date.AddDays(1))
        {
            foreach (var item in prescription.Items)
            {
                foreach (var schedule in item.Schedules)
                {
                    var localDateTime =
                        date.Add(schedule.DosageTime);

                    var scheduledAtUtc =
                        TimeZoneInfo.ConvertTimeToUtc(
                            DateTime.SpecifyKind(
                                localDateTime,
                                DateTimeKind.Unspecified),
                            egyptTimeZone);

                    if (scheduledAtUtc <= DateTime.UtcNow)
                    {
                        continue;
                    }

                    await _dosageReminderScheduler.ScheduleAsync(
                        schedule.Id,
                        scheduledAtUtc);
                }
            }
        }
    }
}