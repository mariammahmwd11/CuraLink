using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;

namespace CuraLink.Infrastructure.BackgroundJobs;

public class DosageReminderJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public DosageReminderJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Execute(Guid dosageScheduleId)
    {
        var schedule =
            await _unitOfWork.Prescriptions
                .GetDosageScheduleWithDetailsAsync(
                    dosageScheduleId);

        if (schedule == null)
        {
            return;
        }

        var prescription =
            schedule.PrescriptionItem.Prescription;

        if (!prescription.IsActive)
        {
            return;
        }

        var patient =
            prescription.Patient;

        await _notificationService.SendAsync(
            patient.ApplicationUserId,
            "Medication Reminder",
            $"Time to take {schedule.PrescriptionItem.MedicationName}. " +
            $"Dosage: {schedule.PrescriptionItem.Dosage}" +
            (string.IsNullOrWhiteSpace(
                schedule.PrescriptionItem.Instructions)
                ? ""
                : $" - {schedule.PrescriptionItem.Instructions}")
        );
    }
}