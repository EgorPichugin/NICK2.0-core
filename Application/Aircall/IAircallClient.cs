namespace NICK2._0_core.Application.Aircall;

/// <summary>
/// Provides access to Aircall API resources.
/// </summary>
public interface IAircallClient
{
    /// <summary>
    /// Gets calls and their AI summaries by customer phone number.
    /// </summary>
    /// <param name="customerPhoneNumber">The customer phone number used to search calls.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matched calls with AI summary content.</returns>
    Task<IReadOnlyCollection<AircallCall>> GetCalls(
        string customerPhoneNumber,
        CancellationToken cancellationToken);
}
