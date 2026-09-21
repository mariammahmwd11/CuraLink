using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Queries.GetSubscriptionStatus;

public record GetNotificationSubscriptionStatusQuery(string UserId) : IRequest<bool>;

public class GetNotificationSubscriptionStatusQueryHandler
    : IRequestHandler<GetNotificationSubscriptionStatusQuery, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationSubscriptionStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        GetNotificationSubscriptionStatusQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients
            .GetByApplicationUserIdAsync(request.UserId, cancellationToken);

        if (patient == null)
        {
            return false;
        }

        var subscriptions = await _unitOfWork.NotificationSubscriptions
            .GetByPatientIdAsync(patient.Id, cancellationToken);

        return subscriptions.Count > 0;
    }
}
