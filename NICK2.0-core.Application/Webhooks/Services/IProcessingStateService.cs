using NICK2._0_core.Application.Webhooks.Commands;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <summary>
/// Tracks webhook processing state to prevent duplicate processing.
/// </summary>
public interface IProcessingStateService
{
    /// <summary>
    /// Attempts to mark a webhook event as started.
    /// </summary>
    /// <param name="command">The webhook command to track.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><c>true</c> when processing can continue; otherwise, <c>false</c>.</returns>
    Task<bool> TryStartProcessing(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks a webhook event as successfully processed.
    /// </summary>
    /// <param name="command">The webhook command to track.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task MarkProcessed(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken);
}
