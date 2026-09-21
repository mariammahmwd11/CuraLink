using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Notifications.Common;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(string UserId) : IRequest<List<NotificationDto>>;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients
            .GetByApplicationUserIdAsync(request.UserId, cancellationToken);

        if (patient == null)
        {
            return new List<NotificationDto>();
        }

        var notifications = await _unitOfWork.Notifications
            .GetByPatientIdAsync(patient.Id, cancellationToken);

        return notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToList();
    }
}
