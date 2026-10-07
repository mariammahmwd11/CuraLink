using CuraLink.Application.Common.Interfaces.AI;
using CuraLink.Infrastructure.Services.AI.DrugData.OpenFDA;
using CuraLink.Infrastructure.Services.AI.Gemini;

namespace CuraLink.Infrastructure.Services.AI;

public class DrugAssistantService : IDrugAssistantService
{
    private readonly GeminiService _geminiService;
    private readonly OpenFDAService _openFDAService;

    public DrugAssistantService(
        GeminiService geminiService,
        OpenFDAService openFDAService)
    {
        _geminiService = geminiService;
        _openFDAService = openFDAService;
    }

    public async Task<DrugAssistantResponse> AskAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // 1. Understand the user's question
        var analysis = await _geminiService.AnalyzeQueryAsync(
            query,
            cancellationToken);

        // 2. No medication identified
        if (analysis.Medications.Count == 0)
        {
            return new DrugAssistantResponse
            {
                Answer =
                    "I couldn't identify a medication in your question. " +
                    "Please mention the medication name and tell me what you'd like to know about it."
            };
        }

        // 3. Retrieve trusted FDA information
        var drugLabels = new List<OpenFDADrugLabel>();

        foreach (var medication in analysis.Medications)
        {
            var drugLabel = await _openFDAService.GetDrugLabelAsync(
                medication,
                cancellationToken);

            if (drugLabel is not null)
            {
                drugLabels.Add(drugLabel);
            }
        }

        // 4. No FDA information found
        if (drugLabels.Count == 0)
        {
            return new DrugAssistantResponse
            {
                Answer =
                    $"I couldn't find an FDA drug label for " +
                    $"{string.Join(", ", analysis.Medications)}. " +
                    "Please check the medication name and try again."
            };
        }

        // 5. Let Gemini turn the trusted FDA data
        //    into a natural conversational answer
        return await _geminiService.GenerateDrugResponseAsync(
            query,
            drugLabels,
            cancellationToken);
    }
}