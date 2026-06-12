using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Webhooks;

/// <summary>
/// Coordinates processing of SmartMoving webhook events.
/// </summary>
public interface IWebHookProcessingService
{
    /// <summary>
    /// Processes an opportunity status changed webhook event.
    /// </summary>
    /// <param name="command">The command that contains the webhook event data.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<SmartMovingOpportunity?> ProcessOpportunityStatusChanged(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken);
}
