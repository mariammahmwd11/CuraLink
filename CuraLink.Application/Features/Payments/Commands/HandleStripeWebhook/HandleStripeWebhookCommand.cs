using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Payments.Commands.HandleStripeWebhook
{
    public record HandleStripeWebhookCommand(
    string SessionId,
    string PaymentId,
    string? TransactionId
) : IRequest;
}
