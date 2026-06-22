using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Aircall;

public sealed record AircallCallAiSummary(
    [property: JsonPropertyName("id")]
    long Id,
    [property: JsonPropertyName("callId")]
    long CallId,
    [property: JsonPropertyName("createdAt")]
    DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("content")]
    string Content);
