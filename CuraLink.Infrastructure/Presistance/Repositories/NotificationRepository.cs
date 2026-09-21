using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Notifications;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Infrastructure.Presistance.Repositories;

// NOTE: This assumes other repositories in this project also take the
// concrete ApplicationDbContext directly. If your existing repositories
// instead take IApplicationDbContext, change the constructor parameter
// type to match — nothing else in this file needs to change.
public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Notification notification)
    {
        _context.Notifications.Add(notification);
    }

    public async Task<List<Notification>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.PatientId == patientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(
                n => n.PatientId == patientId && !n.IsRead,
                cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }
}
