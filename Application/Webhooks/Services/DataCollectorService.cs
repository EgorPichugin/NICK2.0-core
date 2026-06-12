using NICK2._0_core.Infrastructure.SmartMoving;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class DataCollectorService(
    ISmartMovingClient smartMovingClient) : IDataCollectorService
{
    /// <inheritdoc />
    public async Task CollectOpportunityData(Guid opportunityId, CancellationToken cancellationToken)
    {
        await smartMovingClient.GetOpportunity(opportunityId, cancellationToken);
    }
}
