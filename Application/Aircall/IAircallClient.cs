namespace NICK2._0_core.Application.Aircall;

using NICK2._0_core.Application.Developer;

/// <summary>
/// Provides access to Aircall API resources.
/// </summary>
public interface IAircallClient
{
    /// <summary>
    /// Searches Aircall calls by customer phone number and returns the original service-shaped data.
    /// </summary>
    /// <param name="customerPhoneNumber">The customer phone number used to search calls.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The original Aircall search response data.</returns>
    Task<DevAircallCallSearchResponse> SearchCallsForDeveloper(
        string customerPhoneNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the AI summary for an Aircall call using the original service-shaped data.
    /// </summary>
    /// <param name="callId">The Aircall call identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The original Aircall AI summary response data when found; otherwise, <c>null</c>.</returns>
    Task<DevAircallCallAiSummaryResponse?> GetCallAiSummaryForDeveloper(
        long callId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches Aircall calls by customer phone number.
    /// </summary>
    /// <param name="customerPhoneNumber">The customer phone number used to search calls.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matched Aircall calls without AI summaries.</returns>
    Task<IReadOnlyCollection<AircallCallSearchResult>> SearchCalls(
        string customerPhoneNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the AI summary for an Aircall call.
    /// </summary>
    /// <param name="callId">The Aircall call identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The AI summary when found; otherwise, <c>null</c>.</returns>
    Task<AircallCallAiSummary?> GetCallAiSummary(
        long callId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets calls and their AI summaries by customer phone number.
    /// </summary>
    /// <param name="customerPhoneNumber">The customer phone number used to search calls.</param>
    /// <param name="callsFrom">The earliest call start date to include in the search.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matched calls with AI summary content.</returns>
    Task<IReadOnlyCollection<AircallCall>> GetCalls(
        string customerPhoneNumber,
        DateTimeOffset callsFrom,
        CancellationToken cancellationToken);
}
