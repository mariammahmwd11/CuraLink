using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllNotificationsAsReadCommand(
    string UserId) : IRequest<bool>;

public class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<
        MarkAllNotificationsAsReadCommand,
        bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notifications =
            await _unitOfWork.Notifications
                .GetByUserIdAsync(
                    request.UserId,
                    cancellationToken);

        var unread =
            notifications
                .Where(n => !n.IsRead)
                .ToList();

        if (unread.Count == 0)
        {
            return true;
        }

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}