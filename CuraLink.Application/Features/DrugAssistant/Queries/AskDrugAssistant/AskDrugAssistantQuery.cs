using MediatR;

namespace CuraLink.Application.Features.DrugAssistant.Queries.AskDrugAssistant
{
    public record AskDrugAssistantQuery(string Query)
     : IRequest<DrugAssistantResponse>;
}
