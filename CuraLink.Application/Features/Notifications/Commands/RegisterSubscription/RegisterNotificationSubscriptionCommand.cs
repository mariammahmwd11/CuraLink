using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Notifications.Commands.RegisterSubscription
{
    public record RegisterNotificationSubscriptionCommand(
     string UserId,
     string Endpoint,
     string P256DH,
     string Auth
 ) : IRequest;
}
