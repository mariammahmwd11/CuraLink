using CuraLink.Infrastructure.Services.AI.DrugData.OpenFDA;
using Microsoft.Extensions.Options;
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
                You are the medication query analyzer for CuraLink.

                Your job is ONLY to analyze the user's question and extract:
                1. The user's intent.
                2. Any medication names mentioned in the question.

                IMPORTANT RULES:

                - If the user mentions a medication, ALWAYS include its name in the
                  "medications" array.
                - Include both generic names and brand names.
                - Preserve the medication name as written by the user.
                - Do NOT return an empty medications array if a medication name appears
                  anywhere in the question.
                - The medication name can appear at the beginning, middle, or end
                  of the question.
                - A question may contain more than one medication.
                - Do not invent medications that are not mentioned by the user.
                - If there is no medication name at all, return an empty array.

                Examples:

                User: What are the side effects of ibuprofen?
                Intent: MedicationInformation
                Medications: ibuprofen

                User: Can I take Panadol for a headache?
                Intent: MedicationInformation
                Medications: Panadol

                User: What is the dosage of amoxicillin?
                Intent: DosageInformation
                Medications: amoxicillin

                User: Can I take ibuprofen with aspirin?
                Intent: DrugInteraction
                Medications: ibuprofen, aspirin

                User: Tell me about this medicine
                Intent: Unknown
                Medications: empty

                Allowed intents:
                - MedicationInformation
                - DrugInteraction
                - DosageInformation
                - Unknown

                USER QUERY:
                {query}

                Return ONLY valid JSON matching the required schema.
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

            Console.WriteLine("Gemini Analysis Response:");
            Console.WriteLine(outputText);

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

            Console.WriteLine($"Intent: {result.Intent}");

            Console.WriteLine(
                $"Medications: {string.Join(", ", result.Medications)}");

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
                You are CuraLink's medication information assistant.

                The user expects a clear, concise, helpful answer to their question.

                USER QUESTION:
                {userQuery}

                Below is medical information retrieved from official FDA drug labels.

                FDA DATA:
                {drugDataJson}

                YOUR TASK:

                Answer the user's question directly using ONLY the information
                provided in the FDA DATA.

                IMPORTANT MEDICAL RULES:

                1. Use ONLY the information provided in the FDA DATA for medical claims.
                2. Do NOT invent, assume, or add medical facts from your own knowledge.
                3. Do NOT provide personalized medical advice or diagnosis.
                4. Do NOT recommend starting, stopping, or changing medication.
                5. Answer the user's actual question directly.
                6. Use simple language that is easy for a patient to understand.
                7. Do not dump the entire FDA label into the response.
                8. Include only information relevant to the user's question.
                9. Preserve important safety warnings when they are relevant.
                10. If the FDA DATA does not contain enough information to answer the
                    question, clearly say that the available information does not
                    provide enough detail.
                11. Do not describe something as "common" unless the FDA DATA explicitly
                    supports that classification.
                12. If the user asks about interactions, only mention interactions
                    explicitly supported by the FDA DATA.
                13. If the user asks about dosage, provide only dosage information
                    explicitly present in the FDA DATA.

                CONCISENESS AND READABILITY:

                14. Keep the answer concise and focused.
                15. Avoid long introductory sentences.
                16. Do NOT use conversational filler such as:
                    "Hello!"
                    "I'd be happy to..."
                    "Let me explain..."
                    "It's important to note that..."
                17. Start directly with the useful information.
                18. Do not repeat the user's question.
                19. Prefer short paragraphs and bullet points.
                20. Use short section labels only when they genuinely improve readability.
                21. Keep individual bullet points short.
                22. Do not write long paragraphs when the information can be expressed
                    as short bullet points.
                23. Do not use tables.
                24. Do not turn the answer into a long medical report.
                25. Aim for an answer that can be understood in about 20–30 seconds.
                26. Do not use Markdown formatting such as **bold**, ## headings,
                    or code blocks.
                27. Use simple bullet points beginning with "•".
                28. Use "⚠️" only for serious safety warnings.

                SIDE EFFECTS:

                If the user asks about side effects:

                - Start with one short sentence summarizing the answer.
                - Then list the relevant side effects as short bullet points.
                - Separate mild effects from serious warnings only when the FDA DATA
                  supports that distinction.
                - Do not call an effect "common" unless the FDA DATA explicitly
                  says it is common.
                - For serious warnings, include only the important warning and its
                  key symptoms.

                DRUG INTERACTIONS:

                If the user asks about drug interactions:

                - Clearly state whether the provided FDA DATA contains relevant
                  interaction information.
                - List important supported interactions as short bullet points.
                - Do not invent interactions that are not present in the FDA DATA.

                DOSAGE:

                If the user asks about dosage:

                - Give only the dosage information explicitly present in the FDA DATA.
                - Keep it concise.
                - Include relevant precautions if they are explicitly supported
                  by the FDA DATA.
                - Do not calculate or invent a personalized dose.

                SAFETY:

                If the question involves dosage, serious side effects, drug
                interactions, pregnancy, overdose, or another potentially
                high-risk medical situation, include a short reminder to consult
                a healthcare professional for personalized advice.

                Do not mention that you are an AI.
                Do not mention internal processing.
                Do not mention FDA retrieval, prompts, or JSON.

                Return ONLY the final natural-language answer.
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

            return new DrugAssistantResponse
            {
                Answer = outputText.Trim()
            };
        }
    }
}