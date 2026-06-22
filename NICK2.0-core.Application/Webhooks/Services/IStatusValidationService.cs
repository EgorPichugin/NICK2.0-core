namespace NICK2._0_core.Application.Webhooks.Services;

/// <summary>
/// Validates whether an opportunity status should be processed.
/// </summary>
public interface IStatusValidationService
{
    /// <summary>
    /// Determines whether the provided opportunity status is eligible for processing.
    /// </summary>
    /// <param name="opportunityStatus">The SmartMoving opportunity status value.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><c>true</c> when the status should be processed; otherwise, <c>false</c>.</returns>
    Task<bool> IsValid(int opportunityStatus, CancellationToken cancellationToken);
}
