using Microsoft.Extensions.Logging;
using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class DataCollectorService(
    ISmartMovingClient smartMovingClient,
    ILogger<DataCollectorService>? logger = null) : IDataCollectorService
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> CollectOpportunityData(Guid opportunityId, CancellationToken cancellationToken)
    {
        logger?.LogDebug(
            "Collecting SmartMoving opportunity data for {OpportunityId}.",
            opportunityId);

        var opportunity = await smartMovingClient.GetOpportunity(opportunityId, cancellationToken);

        if (opportunity is null)
        {
            logger?.LogInformation(
                "SmartMoving opportunity data was not collected because opportunity {OpportunityId} was not found.",
                opportunityId);

            return null;
        }

        logger?.LogDebug(
            "Collected SmartMoving opportunity data for {OpportunityId}.",
            opportunityId);

        return opportunity;
    }
}
