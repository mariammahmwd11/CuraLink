using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Commands.MarkAsRead;

/// <returns>false if the notification doesn't exist or doesn't belong to this patient.</returns>
public record MarkNotificationAsReadCommand(Guid NotificationId, string UserId) : IRequest<bool>;

public class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients
            .GetByApplicationUserIdAsync(request.UserId, cancellationToken);

        if (patient == null)
        {
            return false;
        }

        var notification = await _unitOfWork.Notifications
            .GetByIdAsync(request.NotificationId, cancellationToken);

        // Ownership check: never let a patient mark someone else's notification.
        if (notification == null || notification.PatientId != patient.Id)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
