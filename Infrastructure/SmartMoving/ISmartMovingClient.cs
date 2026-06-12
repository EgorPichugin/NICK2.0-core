namespace NICK2._0_core.Infrastructure.SmartMoving;

/// <summary>
/// Provides access to SmartMoving API resources.
/// </summary>
public interface ISmartMovingClient
{
    /// <summary>
    /// Gets opportunity data from SmartMoving.
    /// </summary>
    /// <param name="opportunityId">The SmartMoving opportunity identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The opportunity data when found; otherwise, <c>null</c>.</returns>
    Task<SmartMovingOpportunity?> GetOpportunity(
        Guid opportunityId,
        CancellationToken cancellationToken);
}
