using CuraLink.Infrastructure.Services.AI.DrugData.OpenFDA;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CuraLink.Infrastructure.Services.AI.Gemini
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;

        public GeminiService(
            HttpClient httpClient,
            IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<DrugQueryAnalysis> AnalyzeQueryAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var prompt = $"""
            You are a medication information assistant.

            Analyze the following user query.

            Determine:
            1. The user's intent.
            2. The medication names mentioned in the query.

            Allowed intents:
            - MedicationInformation
            - DrugInteraction
            - DosageInformation
            - Unknown

            User query:
            {query}

            Return only valid JSON.
            """;

            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },

                generationConfig = new
                {
                    responseMimeType = "application/json",

                    responseSchema = new
                    {
                        type = "OBJECT",

                        properties = new
                        {
                            intent = new
                            {
                                type = "STRING",
                                @enum = new[]
                                {
                                "MedicationInformation",
                                "DrugInteraction",
                                "DosageInformation",
                                "Unknown"
                            }
                            },

                            medications = new
                            {
                                type = "ARRAY",

                                items = new
                                {
                                    type = "STRING"
                                }
                            }
                        },

                        required = new[]
                        {
                        "intent",
                        "medications"
                    }
                    }
                }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/" +
                $"{_options.Model}:generateContent";

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                url);

            request.Headers.Add(
                "x-goog-api-key",
                _options.ApiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini API Error ({(int)response.StatusCode}): {responseBody}");
            }

            using var document =
                JsonDocument.Parse(responseBody);

            var outputText = document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response.");
            }

            var result =
                JsonSerializer.Deserialize<DrugQueryAnalysis>(
                    outputText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Failed to parse Gemini response.");
            }

            return result;
        }
        public async Task<DrugAssistantResponse> GenerateDrugResponseAsync(
     string userQuery,
     List<OpenFDADrugLabel> drugLabels,
     CancellationToken cancellationToken = default)
        {
            var drugDataJson = JsonSerializer.Serialize(
                drugLabels,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var prompt = $"""
        You are a medication information assistant.

        The user asked:
        {userQuery}

        Below is medical information retrieved from FDA drug labels.

        FDA DATA:
        {drugDataJson}

        Your task is to generate a clear, concise, structured response
        for the user.

        IMPORTANT RULES:

        1. Use ONLY the information provided in the FDA DATA.
        2. Do NOT invent or infer medical facts.
        3. Do NOT provide personalized medical advice.
        4. Do NOT recommend changing, starting, or stopping medication.
        5. If information is missing from the FDA DATA, return an empty
           array or null for that field.
        6. Summarize long FDA text into concise user-friendly information.
        7. Preserve important safety warnings.
        8. Only describe an effect as "common" if the FDA DATA explicitly identifies it as common.
        9. Do not classify serious warnings or adverse reactions as common side effects unless the FDA DATA explicitly supports that classification.
        10. If the FDA DATA does not provide enough information to identify common side effects, return an empty commonSideEffects array.
        11. If the user asks about drug interactions, only mention interactions explicitly supported by the FDA DATA.
        12. Always include the medical disclaimer.

        Return only valid JSON matching the required response schema.
        """;

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = prompt
                    }
                }
            }
        },

                generationConfig = new
                {
                    responseMimeType = "application/json",

                    responseSchema = new
                    {
                        type = "OBJECT",

                        properties = new
                        {
                            medication = new
                            {
                                type = "STRING"
                            },

                            activeIngredients = new
                            {
                                type = "ARRAY",
                                items = new
                                {
                                    type = "STRING"
                                }
                            },

                            dosageInformation = new
                            {
                                type = "STRING",
                                nullable = true
                            },

                            commonSideEffects = new
                            {
                                type = "ARRAY",
                                items = new
                                {
                                    type = "STRING"
                                }
                            },

                            interactionWarnings = new
                            {
                                type = "ARRAY",

                                items = new
                                {
                                    type = "OBJECT",

                                    properties = new
                                    {
                                        drug = new
                                        {
                                            type = "STRING"
                                        },

                                        description = new
                                        {
                                            type = "STRING"
                                        }
                                    },

                                    required = new[]
                                    {
                                "drug",
                                "description"
                            }
                                }
                            },

                            warnings = new
                            {
                                type = "ARRAY",

                                items = new
                                {
                                    type = "STRING"
                                }
                            },

                            disclaimer = new
                            {
                                type = "STRING"
                            }
                        },

                        required = new[]
                        {
                    "medication",
                    "activeIngredients",
                    "dosageInformation",
                    "commonSideEffects",
                    "interactionWarnings",
                    "warnings",
                    "disclaimer"
                }
                    }
                }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/" +
                $"{_options.Model}:generateContent";

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                url);

            request.Headers.Add(
                "x-goog-api-key",
                _options.ApiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini API Error ({(int)response.StatusCode}): {responseBody}");
            }

            using var document =
                JsonDocument.Parse(responseBody);

            var outputText = document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response.");
            }

            var result =
                JsonSerializer.Deserialize<DrugAssistantResponse>(
                    outputText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Failed to parse Gemini drug assistant response.");
            }

            return result;
        }
    }
}
