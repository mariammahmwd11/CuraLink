using CuraLink.Application.Features.DrugAssistant.Queries.AskDrugAssistant;
using MediatR;

namespace CuraLink.API.Endpoints.DrugAssistant;

public static class AskDrugAssistantEndpoint
{
    public static void MapAskDrugAssistantEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/drug-assistant/query",
            async (
                AskDrugAssistantQuery query,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    query,
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization();
    }
}