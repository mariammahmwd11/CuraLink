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
        var analysis = await _geminiService.AnalyzeQueryAsync(
            query,
            cancellationToken);

        if (analysis.Medications.Count == 0)
        {
            return new DrugAssistantResponse
            {
                Warnings =
                [
                    "No medication could be identified from the query."
                ],

                Disclaimer =
                    "This information is for educational purposes only and does not replace professional medical advice."
            };
        }

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

        if (drugLabels.Count == 0)
        {
            return new DrugAssistantResponse
            {
                Medication = string.Join(
                    ", ",
                    analysis.Medications),

                Warnings =
                [
                    "No FDA drug label was found for the specified medication(s)."
                ],

                Disclaimer =
                    "This information is for educational purposes only and does not replace professional medical advice."
            };
        }

        return await _geminiService.GenerateDrugResponseAsync(
            query,
            drugLabels,
            cancellationToken);
    
}
}