namespace NICK2._0_core.Application.Llm;

public sealed record LlmOpportunityAnalysisResult(
    Guid OpportunityId,
    bool IsMocked,
    string Summary);
