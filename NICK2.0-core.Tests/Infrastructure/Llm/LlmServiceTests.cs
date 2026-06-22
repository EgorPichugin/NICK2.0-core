using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.Processing;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Domain;
using NICK2._0_core.Infrastructure.Llm;

namespace NICK2._0_core.Tests.Infrastructure.Llm;

public sealed class LlmServiceTests
{
    [Fact]
    public async Task AnalyzeOpportunity_ReturnsLlmResult()
    {
        var opportunity = new SmartMovingOpportunity(
            Guid.NewGuid(),
            90690,
            "7472874522",
            EType.Moving,
            3,
            DateTimeOffset.Parse("2026-06-18T00:00:00Z"),
            null,
            null,
            []);
        var callSummaries = new[]
        {
            new AircallCall(
                812,
                "outbound",
                DateTimeOffset.Parse("2026-06-18T18:35:00Z"),
                DateTimeOffset.Parse("2026-06-18T18:42:00Z"),
                1,
                false,
                "Client asks for a moving estimate.")
        };
        var completionClient = new FakeLlmCompletionClient(
        [
            """
            {
              "facts": [
                {
                  "name": "crew_size",
                  "description": "Requested, quoted, or expected number of movers.",
                  "value": "3 movers",
                  "status": "found",
                  "confidence": "high",
                  "requiresHumanReview": false,
                  "evidence": [
                    {
                      "callId": "812",
                      "quote": "Customer needs three movers."
                    }
                  ],
                  "notes": null
                }
              ],
              "confidence": "high",
              "requiresHumanReview": false
            }
            """,
            """
            {
              "overallStatus": "match",
              "confidence": "high",
              "requiresHumanReview": false,
              "facts": [
                {
                  "name": "crew_size",
                  "description": "Requested, quoted, or expected number of movers.",
                  "callValue": "3 movers",
                  "crewNotesValue": "Crew 3 movers",
                  "status": "match",
                  "confidence": "high",
                  "requiresHumanReview": false,
                  "crewNotesEvidence": "Crew 3 movers",
                  "evidence": [
                    {
                      "callId": "812",
                      "quote": "Customer needs three movers."
                    }
                  ],
                  "explanation": "CrewNotes and call facts both indicate 3 movers."
                }
              ],
              "discrepancies": [],
              "missingFacts": []
            }
            """
        ]);
        var service = new LlmService(
            completionClient,
            Options.Create(new LlmOptions
            {
                ApiKey = "test-key",
                Model = "test-model"
            }),
            NullLogger<LlmService>.Instance);
        var processingData = new OpportunityProcessingData(
            opportunity,
            callSummaries,
            OpportunityFactsCatalog.GetFacts(opportunity.Type));

        var result = await service.AnalyzeOpportunity(processingData, CancellationToken.None);

        Assert.Equal(opportunity, result.Opportunity);
        Assert.Equal(callSummaries, result.AircallCalls);
        Assert.NotNull(result.ExtractedCallFacts);
        Assert.Single(result.ExtractedCallFacts.Facts);
        Assert.Equal("crew_size", result.ExtractedCallFacts.Facts.Single().Name);
        Assert.NotNull(result.AnalysisResult);
        Assert.Equal(opportunity.Id, result.AnalysisResult.OpportunityId);
        Assert.False(result.AnalysisResult.IsMocked);
        Assert.Equal("match", result.AnalysisResult.Summary.OverallStatus);
        Assert.Single(result.AnalysisResult.Summary.Facts);
        Assert.Equal("crew_size", result.AnalysisResult.Summary.Facts.Single().Name);
        Assert.Equal(2, completionClient.Requests.Count);
        Assert.Equal("extracted_call_facts", completionClient.Requests[0].SchemaName);
        Assert.Equal("crew_notes_facts_comparison", completionClient.Requests[1].SchemaName);
        Assert.Contains("Client asks for a moving estimate.", completionClient.Requests[0].Prompt);
        Assert.Contains("3 movers", completionClient.Requests[1].Prompt);
    }

    private sealed class FakeLlmCompletionClient(
        IReadOnlyList<string> responses) : ILlmCompletionClient
    {
        public List<Request> Requests { get; } = [];

        public Task<string> CreateStructuredResponse(
            string prompt,
            string schemaName,
            object schema,
            CancellationToken cancellationToken)
        {
            Requests.Add(new Request(prompt, schemaName, schema));
            if (Requests.Count > responses.Count)
            {
                throw new InvalidOperationException("Unexpected LLM completion request.");
            }

            return Task.FromResult(responses[Requests.Count - 1]);
        }

        public sealed record Request(
            string Prompt,
            string SchemaName,
            object Schema);
    }
}
