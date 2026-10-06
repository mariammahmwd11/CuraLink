using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadNotificationCountQuery(
    string UserId) : IRequest<int>;

public class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<
        GetUnreadNotificationCountQuery,
        int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadNotificationCountQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.Notifications
            .GetUnreadCountAsync(
                request.UserId,
                cancellationToken);
    }
}