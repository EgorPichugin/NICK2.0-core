using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

public sealed class OpportunityStatusSnapshotService(
    IOpportunityStatusSnapshotRepository repository) : IOpportunityStatusSnapshotService
{
    public async Task<bool> UpdateAndCheckShouldProcess(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        var currentStatus = (OpportunityStatus)command.OpportunityStatus;
        var previousStatus = await repository.UpsertStatus(
            command.OpportunityId,
            currentStatus,
            cancellationToken);

        return currentStatus == OpportunityStatus.Booked
            && previousStatus is OpportunityStatus.Opportunity or OpportunityStatus.Lost;
    }
}
