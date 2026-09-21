using CuraLink.Domain.Entities.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface INotificationSubscriptionRepository
    {
        Task<PatientNotificationSubscription?> GetByEndpointAsync(
            string endpoint,
            CancellationToken cancellationToken = default);

        Task<List<PatientNotificationSubscription>> GetByPatientIdAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PatientNotificationSubscription subscription,
            CancellationToken cancellationToken = default);

        void Remove(
            PatientNotificationSubscription subscription);
    }
}
