namespace NICK2._0_core.Application.Llm;

public sealed record LlmOpportunityAnalysisResult(
    Guid OpportunityId,
    bool IsMocked,
    LlmOpportunityAnalysisSummary Summary);

public sealed record LlmOpportunityAnalysisSummary(
    string OverallStatus,
    string Confidence,
    bool RequiresHumanReview,
    IReadOnlyCollection<LlmOpportunityAnalyzedFact> Facts,
    IReadOnlyCollection<LlmOpportunityAnalysisDiscrepancy> Discrepancies,
    IReadOnlyCollection<LlmOpportunityAnalysisMissingFact> MissingFacts);

public sealed record LlmOpportunityAnalyzedFact(
    string Name,
    string Description,
    string? CallValue,
    string? CrewNotesValue,
    string Status,
    string Confidence,
    bool RequiresHumanReview,
    string? CrewNotesEvidence,
    IReadOnlyCollection<ExtractedCallFactEvidence> Evidence,
    string Explanation);

public sealed record LlmOpportunityAnalysisDiscrepancy(
    string FactName,
    string Issue,
    string? CallValue,
    string? CrewNotesValue,
    string Severity);

public sealed record LlmOpportunityAnalysisMissingFact(
    string FactName,
    string Reason);
