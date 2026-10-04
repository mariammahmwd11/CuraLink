using CuraLink.Application.Common.Interfaces.AI;
using MediatR;

namespace CuraLink.Application.Features.DrugAssistant.Queries.AskDrugAssistant
{
    public class AskDrugAssistantHandler
    : IRequestHandler<AskDrugAssistantQuery, DrugAssistantResponse>
    {
        private readonly IDrugAssistantService _drugAssistantService;

        public AskDrugAssistantHandler(
            IDrugAssistantService drugAssistantService)
        {
            _drugAssistantService = drugAssistantService;
        }

        public async Task<DrugAssistantResponse> Handle(
            AskDrugAssistantQuery request,
            CancellationToken cancellationToken)
        {
            return await _drugAssistantService.AskAsync(
                request.Query,
                cancellationToken);
        }
    }
}
