using System.Security.Claims;
using CuraLink.Application.Features.ClinicAssistants.Queries.SearchPatients;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class SearchPatientsEndpoint
{
    public static void MapSearchPatientsEndpoint(
        this WebApplication app)
    {
        app.MapGet(
            "/api/clinic-assistants/patients/search",
            async (
                string? search,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var applicationUserId =
                    user.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(applicationUserId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(search))
                    return Results.BadRequest(new
                    {
                        message = "Search term is required."
                    });

                var result = await sender.Send(
                    new SearchPatientsQuery(
                        applicationUserId,
                        search),
                    cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization("Receptionist").WithTags("Clinic Assistants");
    }
}