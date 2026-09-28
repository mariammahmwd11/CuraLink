using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Payments.Commands.HandleCheckoutSessionExpired
{
    public record HandleCheckoutSessionExpiredCommand(string PaymentId) : IRequest;
}
