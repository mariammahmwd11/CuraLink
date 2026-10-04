public class DrugAssistantResponse
{
    public string Medication { get; set; } = string.Empty;

    public List<string> ActiveIngredients { get; set; } = [];

    public string? DosageInformation { get; set; }

    public List<string> CommonSideEffects { get; set; } = [];

    public List<DrugInteractionDto> InteractionWarnings { get; set; } = [];

    public List<string> Warnings { get; set; } = [];

    public string Disclaimer { get; set; } = string.Empty;
}

public class DrugInteractionDto
{
    public string Drug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}