using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.ClinicAssistants.Commands.RegisterClinicAssistant;
using CuraLink.Domain.Entities.ClinicAssistants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.RegisterClinicAssistant;

public class RegisterClinicAssistantCommandHandler
    : IRequestHandler<RegisterClinicAssistantCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public RegisterClinicAssistantCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<string> Handle(
        RegisterClinicAssistantCommand request,
        CancellationToken cancellationToken)
    {
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

        var existingUser =
            await _identityService.GetUserIdByEmailAsync(
                invitation.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists. Please login.");
        }

        var result = await _identityService.CreateReceptionistAsync(
            request.FirstName,
            request.LastName,
            invitation.Email,
            request.Phone,
            request.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", result.Errors));
        }

        return result.UserId;
    }
}