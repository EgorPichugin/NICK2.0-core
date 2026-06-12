using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <summary>
/// Collects additional data required to process webhook events.
/// </summary>
public interface IDataCollectorService
{
    /// <summary>
    /// Collects SmartMoving opportunity data for the provided opportunity.
    /// </summary>
    /// <param name="opportunityId">The SmartMoving opportunity identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<SmartMovingOpportunity?> CollectOpportunityData(Guid opportunityId, CancellationToken cancellationToken);
}
