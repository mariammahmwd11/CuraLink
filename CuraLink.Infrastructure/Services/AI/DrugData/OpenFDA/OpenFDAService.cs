using System.Text.Json;

namespace CuraLink.Infrastructure.Services.AI.DrugData.OpenFDA;

public class OpenFDAService
{
    private readonly HttpClient _httpClient;

    public OpenFDAService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OpenFDADrugLabel?> GetDrugLabelAsync(
        string medication,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"https://api.fda.gov/drug/label.json" +
            $"?search=openfda.generic_name:{Uri.EscapeDataString(medication)}" +
            $"&limit=1";

        using var response = await _httpClient.GetAsync(
            url,
            cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenFDA API Error ({(int)response.StatusCode}): {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);

        var results = document.RootElement.GetProperty("results");

        if (results.GetArrayLength() == 0)
        {
            return null;
        }

        var result = results[0];

        return new OpenFDADrugLabel
        {
            BrandName = GetFirstValue(
                result,
                "openfda",
                "brand_name"),

            GenericName = GetFirstValue(
                result,
                "openfda",
                "generic_name"),

            ActiveIngredients = GetStringArray(
                result,
                "active_ingredient"),

            DosageAndAdministration = GetStringArray(
                result,
                "dosage_and_administration"),

            Warnings = GetStringArray(
                result,
                "warnings"),

            AdverseReactions = GetStringArray(
                result,
                "adverse_reactions"),

            DrugInteractions = GetStringArray(
                result,
                "drug_interactions"),

            IndicationsAndUsage = GetStringArray(
                result,
                "indications_and_usage")
        };
    }

    private static List<string> GetStringArray(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return [];
        }

        if (property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property
            .EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString()!)
            .ToList();
    }

    private static string? GetFirstValue(
        JsonElement element,
        string parentProperty,
        string propertyName)
    {
        if (!element.TryGetProperty(
                parentProperty,
                out var parent))
        {
            return null;
        }

        if (!parent.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.Array ||
            property.GetArrayLength() == 0)
        {
            return null;
        }

        return property[0].GetString();
    }
}