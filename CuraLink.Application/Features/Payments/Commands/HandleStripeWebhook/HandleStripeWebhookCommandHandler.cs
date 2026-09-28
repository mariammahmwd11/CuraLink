using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Payments.Commands.HandleStripeWebhook;

public class HandleStripeWebhookCommandHandler
    : IRequestHandler<HandleStripeWebhookCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public HandleStripeWebhookCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
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
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
            throw new KeyNotFoundException("Payment not found.");

        // Webhooks can be sent more than once.
        if (payment.Status == PaymentStatus.Paid)
            return;

        // الفلوس اتسحبت، فالـ Payment يتسجل Paid في كل الأحوال
        payment.Status = PaymentStatus.Paid;
        payment.TransactionId = request.TransactionId;
        payment.PaidAt = DateTime.UtcNow;

        // الـ Appointment بنحوّلها Paid بس لو لسه Pending.
        // لو كانت Cancelled (حالة نادرة جداً) منرجعهاش تلقائياً لأن السلوت ممكن يكون اتحجز.
        // بتحتاج refund يدوي.
        if (payment.Appointment.Status == AppointmentStatus.Pending)
        {
            payment.Appointment.Status = AppointmentStatus.Paid;
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (payment.Appointment.Status == AppointmentStatus.Paid)
        {
            await _notificationService.SendAsync(
                payment.Appointment.Patient.ApplicationUserId,
                "Payment Successful",
                "Your appointment has been paid successfully.",
                cancellationToken);
        }
    }
}