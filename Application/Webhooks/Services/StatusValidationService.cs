using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Webhooks.Services;

/// <inheritdoc />
public sealed class StatusValidationService : IStatusValidationService
{
    /// <inheritdoc />
    public Task<bool> IsValid(int opportunityStatus, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enum.IsDefined(typeof(OpportunityStatus), opportunityStatus));
    }
}
