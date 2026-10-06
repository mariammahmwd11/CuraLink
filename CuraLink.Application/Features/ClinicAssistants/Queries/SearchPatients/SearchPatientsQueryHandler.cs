using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.SearchPatients;

public class SearchPatientsQueryHandler
    : IRequestHandler<
        SearchPatientsQuery,
        IReadOnlyList<PatientSearchResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public SearchPatientsQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<PatientSearchResult>> Handle(
        SearchPatientsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get the logged-in receptionist
        var assistant = await _context.ClinicAssistants
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationUserId == request.ApplicationUserId &&
                    x.IsActive,
                cancellationToken);

        if (assistant is null)
        {
            throw new InvalidOperationException(
                "Clinic assistant was not found.");
        }

        // 2. Get the clinic and its doctor
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(
                x => x.Id == assistant.ClinicId,
                cancellationToken);

        if (clinic is null)
        {
            throw new InvalidOperationException(
                "Clinic was not found.");
        }

        // 3. Get patients associated with this clinic's doctor
        var patientUserIds = await _context.DoctorPatients
            .Where(x => x.DoctorId == clinic.DoctorId)
            .Select(x => x.Patient.ApplicationUserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (patientUserIds.Count == 0)
        {
            return Array.Empty<PatientSearchResult>();
        }

        // 4. Search their Identity users
        return await _identityService.SearchPatientsAsync(
            patientUserIds,
            request.Search,
            cancellationToken);
    }
}