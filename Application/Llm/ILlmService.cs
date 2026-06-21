using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Llm;

public interface ILlmService
{
    Task<LlmOpportunityAnalysisResult> AnalyzeOpportunity(
        SmartMovingOpportunity opportunity,
        CancellationToken cancellationToken);
}
