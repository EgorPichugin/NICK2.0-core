using NICK2._0_core.Application.Processing;

namespace NICK2._0_core.Application.Llm;

public interface ILlmService
{
    Task<OpportunityProcessingData> AnalyzeOpportunity(
        OpportunityProcessingData processingData,
        CancellationToken cancellationToken);
}
