using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Repositories;

public interface IOpportunityStatusSnapshotRepository
{
    Task<OpportunityStatus?> UpsertStatus(
        Guid opportunityId,
        OpportunityStatus currentStatus,
        CancellationToken cancellationToken);
}
