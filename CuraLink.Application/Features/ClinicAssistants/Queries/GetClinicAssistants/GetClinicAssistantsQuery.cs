using MediatR;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistants;

public record GetClinicAssistantsQuery(
    string DoctorUserId,
    Guid ClinicId
) : IRequest<IReadOnlyList<ClinicAssistantDto>>;