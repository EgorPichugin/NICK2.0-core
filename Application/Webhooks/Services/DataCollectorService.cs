using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class DataCollectorService(
    ISmartMovingClient smartMovingClient) : IDataCollectorService
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> CollectOpportunityData(Guid opportunityId, CancellationToken cancellationToken)
    {
        return await smartMovingClient.GetOpportunity(opportunityId, cancellationToken);
    }
}
