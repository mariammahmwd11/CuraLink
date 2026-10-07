using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantDashboard;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class GetClinicAssistantDashboardEndpoint
{
    public static void MapGetClinicAssistantDashboardEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinic-assistants/dashboard",
            async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetClinicAssistantDashboardQuery(
                        applicationUserId),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization("Receptionist").WithTags("Clinic Assistants");
    }
}