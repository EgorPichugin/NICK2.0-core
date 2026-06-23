using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Processing;

public sealed record OpportunityProcessingData(
    SmartMovingOpportunity Opportunity,
    IReadOnlyCollection<AircallCall> AircallCalls,
    IReadOnlyCollection<FactToExtract> FactsToExtract,
    ExtractedCallFacts? ExtractedCallFacts = null,
    LlmOpportunityAnalysisResult? AnalysisResult = null);
