namespace NICK2._0_core.Infrastructure.Aircall;

/// <summary>
/// Contains Aircall integration settings.
/// </summary>
public sealed class AircallOptions
{
    /// <summary>
    /// Configuration section name used to bind Aircall settings.
    /// </summary>
    public const string SectionName = "Aircall";

    /// <summary>
    /// Aircall API base URL.
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.aircall.io/v1/";

    /// <summary>
    /// API identifier used for Aircall basic authentication.
    /// </summary>
    public string ApiId { get; init; } = string.Empty;

    /// <summary>
    /// API token used for Aircall basic authentication.
    /// </summary>
    public string ApiToken { get; init; } = string.Empty;

    /// <summary>
    /// Relative path used to search calls.
    /// </summary>
    public string CallsPath { get; init; } = "calls/search";

    /// <summary>
    /// Query parameter name used to pass the customer phone number.
    /// </summary>
    public string PhoneNumberQueryParameter { get; init; } = "phone_number";

    /// <summary>
    /// Relative path template used to get a call AI summary by call id.
    /// </summary>
    public string CallSummaryPathTemplate { get; init; } = "calls/{0}/summary";
}
