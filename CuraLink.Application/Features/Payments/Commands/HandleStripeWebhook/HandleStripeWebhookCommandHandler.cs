
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Patients;
using CuraLink.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Payments.Commands.HandleStripeWebhook;

public class HandleStripeWebhookCommandHandler
    : IRequestHandler<HandleStripeWebhookCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public HandleStripeWebhookCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.PaymentId, out var paymentId))
            throw new ArgumentException("Invalid payment id.");

        var payment = await _context.Payments
            .Include(x => x.Appointment)
                .ThenInclude(x => x.Patient)
            .FirstOrDefaultAsync(
                x => x.Id == paymentId,
                cancellationToken);

        if (payment is null)
            throw new KeyNotFoundException("Payment not found.");

        var wasAlreadyPaid =
            payment.Status == PaymentStatus.Paid;

        // Update payment only when it hasn't been processed before.
        if (!wasAlreadyPaid)
        {
            payment.Status = PaymentStatus.Paid;
            payment.TransactionId = request.TransactionId;
            payment.PaidAt = DateTime.UtcNow;
        }

        var appointment = payment.Appointment;

        // Change Pending appointment to Paid.
        if (appointment.Status == AppointmentStatus.Pending)
        {
            appointment.Status = AppointmentStatus.Paid;
        }

        // Add the patient-doctor relationship only for paid appointments.
        if (appointment.Status == AppointmentStatus.Paid)
        {
            var existingRelationship =
                await _unitOfWork.DoctorPatients.GetAsync(
                    appointment.DoctorId,
                    appointment.PatientId,
                    cancellationToken);

            if (existingRelationship is null)
            {
                var doctorPatient = new DoctorPatient
                {
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId
                };

                await _unitOfWork.DoctorPatients.AddAsync(
                    doctorPatient,
                    cancellationToken);
            }
        }

        // Save payment, appointment, and relationship together.
        await _context.SaveChangesAsync(cancellationToken);

        // Avoid sending duplicate notifications for repeated webhooks.
        if (!wasAlreadyPaid &&
            appointment.Status == AppointmentStatus.Paid)
        {
            await _notificationService.SendAsync(
                appointment.Patient.ApplicationUserId,
                "Payment Successful",
                "Your appointment has been paid successfully.",
                cancellationToken);
        }
    }
}