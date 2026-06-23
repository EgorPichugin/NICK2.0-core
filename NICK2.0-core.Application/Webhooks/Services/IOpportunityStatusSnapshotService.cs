using NICK2._0_core.Application.Webhooks.Commands;

namespace NICK2._0_core.Application.Webhooks.Services;

public interface IOpportunityStatusSnapshotService
{
    Task<bool> UpdateAndCheckShouldProcess(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken);
}
