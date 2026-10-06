using CuraLink.Domain.Entities.Notifications;

namespace CuraLink.Application.Common.Interfaces.Presistence;

public interface INotificationRepository
{
    void Add(Notification notification);

    Task<List<Notification>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}