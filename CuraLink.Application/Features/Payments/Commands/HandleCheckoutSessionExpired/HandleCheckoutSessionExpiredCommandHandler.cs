using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Payments.Commands.HandleCheckoutSessionExpired;

public class HandleCheckoutSessionExpiredCommandHandler
    : IRequestHandler<HandleCheckoutSessionExpiredCommand>
{
    private readonly IApplicationDbContext _context;

    public HandleCheckoutSessionExpiredCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        HandleCheckoutSessionExpiredCommand request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.PaymentId, out var paymentId))
            return;

        var payment = await _context.Payments
            .Include(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        // مدفوعة أو مش موجودة: مفيش حاجة نعملها (idempotent)
        if (payment is null || payment.Status == PaymentStatus.Paid)
            return;

        payment.Status = PaymentStatus.Failed;

        if (payment.Appointment.Status == AppointmentStatus.Pending)
            payment.Appointment.Status = AppointmentStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);
    }
}