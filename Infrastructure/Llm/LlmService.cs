using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.Processing;
using NICK2._0_core.Application.SmartMoving;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NICK2._0_core.Infrastructure.Llm;

public sealed class LlmService(
    ILlmCompletionClient llmCompletionClient,
    IOptions<LlmOptions> options,
    ILogger<LlmService> logger) : ILlmService
{
    private const string CallFactsExtractionPromptFileName = "call-facts-extraction.md";

    private const string CrewNotesFactsComparisonPromptFileName = "crew-notes-facts-comparison.md";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<OpportunityProcessingData> AnalyzeOpportunity(
        OpportunityProcessingData processingData,
        CancellationToken cancellationToken)
    {
        var opportunity = processingData.Opportunity;
        logger.LogDebug(
            "Running LLM analysis for opportunity {OpportunityId} with model {Model} using base URL {BaseUrl}.",
            opportunity.Id,
            options.Value.Model,
            options.Value.BaseUrl);

        var extractedCallFacts = await ExtractFactsFromCall(
            processingData.FactsToExtract,
            processingData.AircallCalls,
            cancellationToken);
        var analysis = await Analyze(
            opportunity,
            extractedCallFacts,
            cancellationToken);

        return processingData with
        {
            ExtractedCallFacts = extractedCallFacts,
            AnalysisResult = analysis
        };
    }

    private async Task<ExtractedCallFacts> ExtractFactsFromCall(
        IReadOnlyCollection<FactToExtract> factsToExtract,
        IReadOnlyCollection<AircallCall> callSummaries,
        CancellationToken cancellationToken)
    {
        var promptTemplate = await ReadPrompt(CallFactsExtractionPromptFileName, cancellationToken);
        var prompt = promptTemplate
            .Replace("{{FACTS_TO_EXTRACT}}", SerializeForPrompt(factsToExtract))
            .Replace("{{CALLS}}", SerializeForPrompt(callSummaries));

        var responseJson = await llmCompletionClient.CreateStructuredResponse(
            prompt,
            "extracted_call_facts",
            BuildExtractedCallFactsSchema(),
            cancellationToken);

        return JsonSerializer.Deserialize<ExtractedCallFacts>(responseJson, JsonOptions)
            ?? throw new InvalidOperationException("LLM returned empty ExtractedCallFacts response.");
    }

    private async Task<LlmOpportunityAnalysisResult> Analyze(
        SmartMovingOpportunity opportunity,
        ExtractedCallFacts extractedCallFacts,
        CancellationToken cancellationToken)
    {
        var promptTemplate = await ReadPrompt(CrewNotesFactsComparisonPromptFileName, cancellationToken);
        var prompt = promptTemplate
            .Replace("{{EXTRACTED_CALL_FACTS}}", SerializeForPrompt(extractedCallFacts))
            .Replace("{{SMARTMOVING_OPPORTUNITY}}", SerializeForPrompt(opportunity));
        var comparisonJson = await llmCompletionClient.CreateStructuredResponse(
            prompt,
            "crew_notes_facts_comparison",
            BuildCrewNotesFactsComparisonSchema(),
            cancellationToken);
        var summary = JsonSerializer.Deserialize<LlmOpportunityAnalysisSummary>(comparisonJson, JsonOptions)
            ?? throw new InvalidOperationException("LLM returned empty opportunity analysis response.");

        return new LlmOpportunityAnalysisResult(
            opportunity.Id,
            false,
            summary);
    }

    private static async Task<string> ReadPrompt(
        string fileName,
        CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Llm", "Prompts", fileName),
            Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Llm", "Prompts", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Llm", "Prompts", fileName)
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return await File.ReadAllTextAsync(path, cancellationToken);
            }
        }

        throw new FileNotFoundException(
            $"LLM prompt template '{fileName}' was not found.",
            paths[0]);
    }

    private static string SerializeForPrompt<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static object BuildExtractedCallFactsSchema()
    {
        return new
        {
            type = "object",
            additionalProperties = false,
            properties = new Dictionary<string, object>
            {
                ["facts"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new Dictionary<string, object>
                        {
                            ["name"] = new { type = "string" },
                            ["description"] = new { type = "string" },
                            ["value"] = new { type = new[] { "string", "null" } },
                            ["status"] = new { type = "string", @enum = new[] { "found", "not_found", "ambiguous", "conflicting" } },
                            ["confidence"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                            ["requiresHumanReview"] = new { type = "boolean" },
                            ["evidence"] = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new Dictionary<string, object>
                                    {
                                        ["callId"] = new { type = "string" },
                                        ["quote"] = new { type = "string" }
                                    },
                                    required = new[] { "callId", "quote" }
                                }
                            },
                            ["notes"] = new { type = new[] { "string", "null" } }
                        },
                        required = new[]
                        {
                            "name",
                            "description",
                            "value",
                            "status",
                            "confidence",
                            "requiresHumanReview",
                            "evidence",
                            "notes"
                        }
                    }
                },
                ["confidence"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                ["requiresHumanReview"] = new { type = "boolean" }
            },
            required = new[] { "facts", "confidence", "requiresHumanReview" }
        };
    }

    private static object BuildCrewNotesFactsComparisonSchema()
    {
        return new
        {
            type = "object",
            additionalProperties = false,
            properties = new Dictionary<string, object>
            {
                ["overallStatus"] = new { type = "string", @enum = new[] { "match", "mismatch", "partial", "insufficient_call_evidence" } },
                ["confidence"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                ["requiresHumanReview"] = new { type = "boolean" },
                ["facts"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new Dictionary<string, object>
                        {
                            ["name"] = new { type = "string" },
                            ["description"] = new { type = "string" },
                            ["callValue"] = new { type = new[] { "string", "null" } },
                            ["crewNotesValue"] = new { type = new[] { "string", "null" } },
                            ["status"] = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "match",
                                    "mismatch",
                                    "missing_in_crew_notes",
                                    "unverified_by_calls",
                                    "extra_in_crew_notes",
                                    "ambiguous"
                                }
                            },
                            ["confidence"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                            ["requiresHumanReview"] = new { type = "boolean" },
                            ["crewNotesEvidence"] = new { type = new[] { "string", "null" } },
                            ["evidence"] = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    properties = new Dictionary<string, object>
                                    {
                                        ["callId"] = new { type = "string" },
                                        ["quote"] = new { type = "string" }
                                    },
                                    required = new[] { "callId", "quote" }
                                }
                            },
                            ["explanation"] = new { type = "string" }
                        },
                        required = new[]
                        {
                            "name",
                            "description",
                            "callValue",
                            "crewNotesValue",
                            "status",
                            "confidence",
                            "requiresHumanReview",
                            "crewNotesEvidence",
                            "evidence",
                            "explanation"
                        }
                    }
                },
                ["discrepancies"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new Dictionary<string, object>
                        {
                            ["factName"] = new { type = "string" },
                            ["issue"] = new { type = "string" },
                            ["callValue"] = new { type = new[] { "string", "null" } },
                            ["crewNotesValue"] = new { type = new[] { "string", "null" } },
                            ["severity"] = new { type = "string", @enum = new[] { "low", "medium", "high" } }
                        },
                        required = new[] { "factName", "issue", "callValue", "crewNotesValue", "severity" }
                    }
                },
                ["missingFacts"] = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new Dictionary<string, object>
                        {
                            ["factName"] = new { type = "string" },
                            ["reason"] = new
                            {
                                type = "string",
                                @enum = new[] { "unverified_by_calls", "missing_in_crew_notes", "extra_in_crew_notes", "ambiguous" }
                            }
                        },
                        required = new[] { "factName", "reason" }
                    }
                }
            },
            required = new[]
            {
                "overallStatus",
                "confidence",
                "requiresHumanReview",
                "facts",
                "discrepancies",
                "missingFacts"
            }
        };
    }
}
