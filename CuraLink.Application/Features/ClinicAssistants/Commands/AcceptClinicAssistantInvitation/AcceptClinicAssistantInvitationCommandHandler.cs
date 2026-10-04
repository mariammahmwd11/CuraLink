using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.ClinicAssistants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.AcceptClinicAssistantInvitation;

public class AcceptClinicAssistantInvitationCommandHandler
    : IRequestHandler<AcceptClinicAssistantInvitationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public AcceptClinicAssistantInvitationCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task Handle(
        AcceptClinicAssistantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get invitation
        var invitation =
            await _context.ClinicAssistantInvitations
                .FirstOrDefaultAsync(
                    x => x.Token == request.Token,
                    cancellationToken);

        if (invitation is null)
        {
            throw new InvalidOperationException(
                "Invalid invitation.");
        }

        // 2. Check if already accepted
        if (invitation.AcceptedAt.HasValue)
        {
            throw new InvalidOperationException(
                "This invitation has already been accepted.");
        }

        // 3. Check expiration
        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "This invitation has expired.");
        }

        // 4. Get logged-in user
        var userInfo =
            await _identityService.GetUserInfoAsync(
                request.UserId);

        if (userInfo is null)
        {
            throw new InvalidOperationException(
                "User not found.");
        }

        // 5. Make sure logged-in email matches invitation email
        if (!string.Equals(
                userInfo.Value.Email,
                invitation.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "This invitation was not sent to your email.");
        }

        // 6. Add Receptionist role if user doesn't have it
        var isReceptionist =
            await _identityService.IsUserInRoleAsync(
                request.UserId,
                "Receptionist");

        if (!isReceptionist)
        {
            var roleResult =
                await _identityService.AddUserToRoleAsync(
                    request.UserId,
                    "Receptionist");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", roleResult.Errors));
            }
        }

        // 7. Check if user is already linked to this clinic
        var existingAssistant =
            await _context.ClinicAssistants
                .FirstOrDefaultAsync(
                    x => x.ClinicId == invitation.ClinicId &&
                         x.ApplicationUserId == request.UserId,
                    cancellationToken);

        if (existingAssistant is not null)
        {
            throw new InvalidOperationException(
                "You are already an assistant in this clinic.");
        }

        // 8. Create clinic assistant
        var clinicAssistant = new ClinicAssistant
        {
            Id = Guid.NewGuid(),
            ClinicId = invitation.ClinicId,
            ApplicationUserId = request.UserId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ClinicAssistants.Add(clinicAssistant);

        // 9. Mark invitation as accepted
        invitation.AcceptedAt = DateTime.UtcNow;

        // 10. Save
        await _context.SaveChangesAsync(cancellationToken);
    }
}