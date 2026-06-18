using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

public sealed class OpportunityStatusSnapshotService(
    IOpportunityStatusSnapshotRepository repository,
    ILogger<OpportunityStatusSnapshotService>? logger = null) : IOpportunityStatusSnapshotService
{
    public async Task<bool> UpdateAndCheckShouldProcess(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        var currentStatus = (EOpportunityStatus)command.OpportunityStatus;
        var previousStatus = await repository.UpsertStatus(
            command.OpportunityId,
            currentStatus,
            cancellationToken);

        var shouldProcess = currentStatus == EOpportunityStatus.Booked
            && previousStatus is EOpportunityStatus.Opportunity or EOpportunityStatus.Lost;

        logger?.LogInformation(
            "Updated opportunity status snapshot for {OpportunityId}. PreviousStatus: {PreviousStatus}. CurrentStatus: {CurrentStatus}. ShouldProcess: {ShouldProcess}.",
            command.OpportunityId,
            previousStatus,
            currentStatus,
            shouldProcess);

        return shouldProcess;
    }
}
