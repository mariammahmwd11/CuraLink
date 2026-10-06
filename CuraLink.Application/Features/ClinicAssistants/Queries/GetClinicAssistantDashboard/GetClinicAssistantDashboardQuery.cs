using MediatR;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantDashboard;

public record GetClinicAssistantDashboardQuery(
    string ApplicationUserId
) : IRequest<GetClinicAssistantDashboardResponse>;