using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Aircall;

public sealed record AircallCallSearchResult(
    [property: JsonPropertyName("id")]
    long Id,
    [property: JsonPropertyName("directLink")]
    string? DirectLink,
    [property: JsonPropertyName("direction")]
    string Direction,
    [property: JsonPropertyName("status")]
    string? Status,
    [property: JsonPropertyName("startedAt")]
    DateTimeOffset? StartedAt,
    [property: JsonPropertyName("answeredAt")]
    DateTimeOffset? AnsweredAt,
    [property: JsonPropertyName("endedAt")]
    DateTimeOffset? EndedAt,
    [property: JsonPropertyName("duration")]
    int? Duration,
    [property: JsonPropertyName("rawDigits")]
    string? RawDigits);
