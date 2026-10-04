using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantInvitation;

public class GetClinicAssistantInvitationQueryHandler
    : IRequestHandler<
        GetClinicAssistantInvitationQuery,
        GetClinicAssistantInvitationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetClinicAssistantInvitationQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<GetClinicAssistantInvitationResponse> Handle(
        GetClinicAssistantInvitationQuery request,
        CancellationToken cancellationToken)
    {
        var invitation =
            await _context.ClinicAssistantInvitations
                .Include(x => x.Clinic)
                .FirstOrDefaultAsync(
                    x => x.Token == request.Token,
                    cancellationToken);

        if (invitation is null)
        {
            throw new InvalidOperationException(
                "Invalid invitation.");
        }

        if (invitation.AcceptedAt.HasValue)
        {
            throw new InvalidOperationException(
                "This invitation has already been accepted.");
        }

        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "This invitation has expired.");
        }

        var userId =
            await _identityService.GetUserIdByEmailAsync(
                invitation.Email);

        return new GetClinicAssistantInvitationResponse(
            invitation.Email,
            userId is not null,
            invitation.Clinic.ClinicName);
    }
}