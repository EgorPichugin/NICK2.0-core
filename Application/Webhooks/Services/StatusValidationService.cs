using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class StatusValidationService(
    ILogger<StatusValidationService>? logger = null) : IStatusValidationService
{
    /// <inheritdoc />
    public Task<bool> IsValid(int opportunityStatus, CancellationToken cancellationToken)
    {
        var isValid = Enum.IsDefined(typeof(EOpportunityStatus), opportunityStatus);

        if (!isValid)
        {
            logger?.LogInformation(
                "SmartMoving opportunity status {OpportunityStatus} is not mapped to a supported domain status.",
                opportunityStatus);
        }

        return Task.FromResult(isValid);
    }
}
