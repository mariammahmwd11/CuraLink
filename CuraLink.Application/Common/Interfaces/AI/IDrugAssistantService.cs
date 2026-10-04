namespace CuraLink.Application.Common.Interfaces.AI
{
    public interface IDrugAssistantService
    {
        Task<DrugAssistantResponse> AskAsync(
            string query,
            CancellationToken cancellationToken = default);
    }
}
