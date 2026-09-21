using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Notifications;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Notifications.Commands.RegisterSubscription
{
    public class RegisterNotificationSubscriptionCommandHandler
    : IRequestHandler<RegisterNotificationSubscriptionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterNotificationSubscriptionCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(
            RegisterNotificationSubscriptionCommand request,
            CancellationToken cancellationToken)
        {
            var patient = await _unitOfWork.Patients
                .GetByApplicationUserIdAsync(
                    request.UserId,
                    cancellationToken);

            if (patient == null)
            {
                throw new KeyNotFoundException(
                    "Patient not found.");
            }

            var existingSubscription =
                await _unitOfWork.NotificationSubscriptions
                    .GetByEndpointAsync(
                        request.Endpoint,
                        cancellationToken);

            if (existingSubscription != null)
            {
                existingSubscription.PatientId = patient.Id;
                existingSubscription.P256DH = request.P256DH;
                existingSubscription.Auth = request.Auth;
                existingSubscription.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            var subscription =
                new PatientNotificationSubscription
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    Endpoint = request.Endpoint,
                    P256DH = request.P256DH,
                    Auth = request.Auth,
                    CreatedAt = DateTime.UtcNow
                };

            await _unitOfWork.NotificationSubscriptions
                .AddAsync(
                    subscription,
                    cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}
