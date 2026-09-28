using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace CuraLink.Application.Features.Payments.Commands.CreateCheckoutSession;

public class CreateCheckoutSessionCommandHandler
    : IRequestHandler<CreateCheckoutSessionCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly StripeSettings _stripeSettings;

    public CreateCheckoutSessionCommandHandler(
        IApplicationDbContext context,
        IOptions<StripeSettings> stripeOptions)
    {
        _context = context;
        _stripeSettings = stripeOptions.Value;
    }

    public async Task<string> Handle(
        CreateCheckoutSessionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get appointment belonging to the current patient
        var appointment = await _context.Appointments
            .Include(x => x.Clinic)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.AppointmentId &&
                    x.Patient.ApplicationUserId == request.ApplicationUserId,
                cancellationToken);

        if (appointment is null)
            throw new KeyNotFoundException(
                "Appointment not found.");

        // 2. Appointment must still be pending
        if (appointment.Status != AppointmentStatus.Pending)
            throw new InvalidOperationException(
                "Only pending appointments can be paid.");

        // 3. Clinic must exist
        if (appointment.Clinic is null)
            throw new KeyNotFoundException(
                "Clinic not found.");

        // 4. Validate consultation price
        if (appointment.Clinic.ConsultationPrice <= 0)
            throw new InvalidOperationException(
                "Invalid consultation price.");

        // 5. Check existing payment
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(
                x => x.AppointmentId == appointment.Id,
                cancellationToken);

        if (existingPayment?.Status == PaymentStatus.Paid)
            throw new InvalidOperationException(
                "Appointment has already been paid.");

        // 6. Create or reuse payment
        var payment = existingPayment;

        if (payment is null)
        {
            payment = new Payment
            {
                AppointmentId = appointment.Id,
                Amount = appointment.Clinic.ConsultationPrice,
                Currency = "EGP",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(
                payment,
                cancellationToken);
        }
        else
        {
            payment.Amount =
                appointment.Clinic.ConsultationPrice;

            payment.Currency = "EGP";

            payment.Status =
                PaymentStatus.Pending;

            payment.CreatedAt =
                DateTime.UtcNow;
        }

        // Save first so Payment.Id exists
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Configure Stripe
        Stripe.StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        // IMPORTANT:
        // Stripe requires expires_at to be more than 30 minutes
        // from session creation. Use 60 minutes to avoid timing issues.
        var options = new SessionCreateOptions
        {
            Mode = "payment",

            SuccessUrl =
                $"{_stripeSettings.SuccessUrl}" +
                $"?appointmentId={appointment.Id}" +
                $"&session_id={{CHECKOUT_SESSION_ID}}",

            CancelUrl =
                $"{_stripeSettings.CancelUrl}" +
                $"?appointmentId={appointment.Id}",

            ExpiresAt =
                DateTime.UtcNow.AddMinutes(60),

            Metadata = new Dictionary<string, string>
            {
                ["AppointmentId"] =
                    appointment.Id.ToString(),

                ["PaymentId"] =
                    payment.Id.ToString()
            },

            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData =
                        new SessionLineItemPriceDataOptions
                        {
                            Currency =
                                payment.Currency.ToLowerInvariant(),

                            UnitAmount =
                                (long)(payment.Amount * 100),

                            ProductData =
                                new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Medical Consultation"
                                }
                        },

                    Quantity = 1
                }
            }
        };

        // 8. Create Stripe Checkout Session
        var service = new SessionService();

        var session = await service.CreateAsync(
            options,
            cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Url))
            throw new InvalidOperationException(
                "Stripe did not return a checkout URL.");

        // 9. Save Stripe session ID
        payment.StripeSessionId = session.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return session.Url;
    }
}