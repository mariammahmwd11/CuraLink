using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkNotificationAsReadCommand(
    Guid NotificationId,
    string UserId) : IRequest<bool>;

public class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications
            .GetByIdAsync(
                request.NotificationId,
                cancellationToken);

        // Notification doesn't exist
        // or doesn't belong to the current user.
        if (notification == null ||
            notification.UserId != request.UserId)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        return true;
    }
}