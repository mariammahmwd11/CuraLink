using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadNotificationCountQuery(string UserId) : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadNotificationCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients
            .GetByApplicationUserIdAsync(request.UserId, cancellationToken);

        if (patient == null)
        {
            return 0;
        }

        return await _unitOfWork.Notifications
            .GetUnreadCountAsync(patient.Id, cancellationToken);
    }
}
