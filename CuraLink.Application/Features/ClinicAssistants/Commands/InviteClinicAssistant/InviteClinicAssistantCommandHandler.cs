using CuraLink.Application.Common.Interfaces.Email;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.ClinicAssistants;
using CuraLink.Domain.Entities.Clinics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.InviteClinicAssistant;

public class InviteClinicAssistantCommandHandler
    : IRequestHandler<InviteClinicAssistantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public InviteClinicAssistantCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Handle(
        InviteClinicAssistantCommand request,
        CancellationToken cancellationToken)
    {
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(
                x => x.Id == request.ClinicId &&
                     x.Doctor.ApplicationUserId == request.DoctorUserId,
                cancellationToken);

        if (clinic is null)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to manage this clinic.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var pendingInvitationExists =
            await _context.ClinicAssistantInvitations
                .AnyAsync(
                    x => x.ClinicId == request.ClinicId &&
                         x.Email == email &&
                         x.AcceptedAt == null &&
                         x.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

        if (pendingInvitationExists)
        {
            throw new InvalidOperationException(
                "An active invitation already exists for this email.");
        }

        var token = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        var invitation = new ClinicAssistantInvitation
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            Email = email,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };

        _context.ClinicAssistantInvitations.Add(invitation);

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendAssistantInvitationEmailAsync(
            email,
            clinic.ClinicName,
            token);
    }
}