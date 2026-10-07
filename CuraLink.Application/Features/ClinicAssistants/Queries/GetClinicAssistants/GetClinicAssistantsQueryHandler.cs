using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistants;

public class GetClinicAssistantsQueryHandler
    : IRequestHandler<
        GetClinicAssistantsQuery,
        IReadOnlyList<ClinicAssistantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetClinicAssistantsQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<ClinicAssistantDto>> Handle(
        GetClinicAssistantsQuery request,
        CancellationToken cancellationToken)
    {
        // Make sure this clinic belongs to the logged-in doctor.
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ClinicId &&
                    x.Doctor.ApplicationUserId == request.DoctorUserId,
                cancellationToken);

        if (clinic is null)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to access this clinic.");
        }

        var assistants = await _context.ClinicAssistants
            .Where(x => x.ClinicId == request.ClinicId)
            .OrderByDescending(x => x.JoinedAt)
            .ToListAsync(cancellationToken);

        var result = new List<ClinicAssistantDto>();

        foreach (var assistant in assistants)
        {
            var userInfo =
                await _identityService.GetUserInfoAsync(
                    assistant.ApplicationUserId);

            if (userInfo is null)
                continue;

            result.Add(
                new ClinicAssistantDto(
                    assistant.Id,
                    userInfo.Value.FirstName,
                    userInfo.Value.LastName,
                    userInfo.Value.Email,
                    null,
                    assistant.IsActive,
                    assistant.JoinedAt));
        }

        return result;
    }
}