using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Aircall;

public sealed record AircallCall(
    [property: JsonPropertyName("id")]
    long Id,
    [property: JsonPropertyName("direction")]
    string Direction,
    [property: JsonPropertyName("summary")]
    string Summary);
