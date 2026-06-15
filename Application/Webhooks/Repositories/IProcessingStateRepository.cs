using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Repositories;

public interface IProcessingStateRepository
{
    Task<bool> TryAddProcessing(
        string eventType,
        Guid opportunityId,
        OpportunityStatus opportunityStatus,
        CancellationToken cancellationToken);

    Task MarkProcessed(
        string eventType,
        Guid opportunityId,
        OpportunityStatus opportunityStatus,
        CancellationToken cancellationToken);
}
