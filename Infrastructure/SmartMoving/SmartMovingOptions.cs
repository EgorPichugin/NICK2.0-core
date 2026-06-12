namespace NICK2._0_core.Infrastructure.SmartMoving;

/// <summary>
/// Contains SmartMoving integration settings.
/// </summary>
public sealed class SmartMovingOptions
{
    /// <summary>
    /// Configuration section name used to bind SmartMoving settings.
    /// </summary>
    public const string SectionName = "SmartMoving";

    /// <summary>
    /// SmartMoving API base URL.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// API key used when calling SmartMoving.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Token used to authorize incoming SmartMoving webhook requests.
    /// </summary>
    // public string WebhookToken { get; init; } = string.Empty;
}
