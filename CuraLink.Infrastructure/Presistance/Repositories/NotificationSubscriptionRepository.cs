using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class NotificationSubscriptionRepository
    : INotificationSubscriptionRepository
    {
        private readonly IApplicationDbContext _context;

        public NotificationSubscriptionRepository(
            IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PatientNotificationSubscription?>
            GetByEndpointAsync(
                string endpoint,
                CancellationToken cancellationToken = default)
        {
            return await _context
                .PatientNotificationSubscriptions
                .FirstOrDefaultAsync(
                    x => x.Endpoint == endpoint,
                    cancellationToken);
        }

        public async Task<List<PatientNotificationSubscription>>
            GetByPatientIdAsync(
                Guid patientId,
                CancellationToken cancellationToken = default)
        {
            return await _context
                .PatientNotificationSubscriptions
                .Where(x => x.PatientId == patientId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            PatientNotificationSubscription subscription,
            CancellationToken cancellationToken = default)
        {
            await _context
                .PatientNotificationSubscriptions
                .AddAsync(
                    subscription,
                    cancellationToken);
        }

        public void Remove(
            PatientNotificationSubscription subscription)
        {
            _context
                .PatientNotificationSubscriptions
                .Remove(subscription);
        }
    }
}
