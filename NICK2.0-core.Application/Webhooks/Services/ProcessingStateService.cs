using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class ProcessingStateService(
    IProcessingStateRepository repository) : IProcessingStateService
{
    /// <inheritdoc />
    public Task<bool> TryStartProcessing(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        return repository.TryAddProcessing(
            command.EventType,
            command.OpportunityId,
            (EOpportunityStatus)command.OpportunityStatus,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task MarkProcessed(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        return repository.MarkProcessed(
            command.EventType,
            command.OpportunityId,
            (EOpportunityStatus)command.OpportunityStatus,
            cancellationToken);
    }
}
