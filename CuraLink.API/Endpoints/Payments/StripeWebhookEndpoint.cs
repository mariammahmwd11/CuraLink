
using CuraLink.Application.Features.Payments;
using CuraLink.Application.Features.Payments.Commands.HandleStripeWebhook;
using MediatR;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace CuraLink.API.Endpoints.Payments;

public static class StripeWebhookEndpoint
{
    public static void MapStripeWebhookEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/payments/webhook",
            async (
                HttpRequest request,
                ISender sender,
                IOptions<StripeSettings> stripeOptions) =>
            {
                var json = await new StreamReader(
                    request.Body).ReadToEndAsync();

                var signature =
                    request.Headers["Stripe-Signature"];

                Stripe.Event stripeEvent;

                try
                {
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        signature,
                        stripeOptions.Value.WebhookSecret);
                }
                catch
                {
                    return Results.BadRequest();
                }

                if (stripeEvent.Type ==
                    EventTypes.CheckoutSessionCompleted)
                {
                    var session =
                        stripeEvent.Data.Object as Session;

                    if (session?.Metadata is null)
                        return Results.BadRequest();

                    if (!session.Metadata.TryGetValue(
                            "PaymentId",
                            out var paymentId))
                    {
                        return Results.BadRequest();
                    }

                    var transactionId =
                        session.PaymentIntentId;

                    await sender.Send(
                        new HandleStripeWebhookCommand(
                            session.Id,
                            paymentId,
                            transactionId));
                }

                return Results.Ok();
            });
    }
}