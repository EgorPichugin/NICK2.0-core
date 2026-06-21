namespace NICK2._0_core.Application.SmartMoving;

using NICK2._0_core.Application.Developer;

/// <summary>
/// Provides access to SmartMoving API resources.
/// </summary>
public interface ISmartMovingClient
{
    /// <summary>
    /// Gets opportunity data from SmartMoving using the original service-shaped data.
    /// </summary>
    /// <param name="opportunityId">The SmartMoving opportunity identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The original opportunity data when found; otherwise, <c>null</c>.</returns>
    Task<DevSmartMovingOpportunityResponse?> GetOpportunityForDeveloper(
        Guid opportunityId,
        CancellationToken cancellationToken);

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
