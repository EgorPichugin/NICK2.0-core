using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Infrastructure.Llm;

public sealed class LlmService : ILlmService
{
    public Task<LlmOpportunityAnalysisResult> AnalyzeOpportunity(
        SmartMovingOpportunity opportunity,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new LlmOpportunityAnalysisResult(
            opportunity.Id,
            true,
            $"Mocked LLM analysis for opportunity {opportunity.Id}."));
    }
}
