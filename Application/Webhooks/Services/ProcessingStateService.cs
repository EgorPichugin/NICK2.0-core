using System.Collections.Concurrent;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class ProcessingStateService : IProcessingStateService
{
    private readonly ConcurrentDictionary<string, ProcessingState> _states = new();

    /// <inheritdoc />
    public Task<bool> TryStartProcessing(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(command);
        var wasAdded = _states.TryAdd(key, ProcessingState.Processing);

        return Task.FromResult(wasAdded);
    }

    /// <inheritdoc />
    public Task MarkProcessed(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(command);
        _states[key] = ProcessingState.Processed;

        return Task.CompletedTask;
    }

    private static string BuildKey(ProcessOpportunityStatusChangedCommand command)
    {
        return $"{command.EventType}:{command.OpportunityId}:{command.OpportunityStatus}";
    }
}
