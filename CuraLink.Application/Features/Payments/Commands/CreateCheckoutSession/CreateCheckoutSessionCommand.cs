using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Payments.Commands.CreateCheckoutSession
{
    public record CreateCheckoutSessionCommand(
     int AppointmentId,
     string ApplicationUserId
 ) : IRequest<string>;
}
