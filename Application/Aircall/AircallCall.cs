using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Aircall;

public sealed record AircallCall(
    [property: JsonPropertyName("id")]
    long Id,
    [property: JsonPropertyName("direction")]
    string Direction,
    [property: JsonPropertyName("startedAt")]
    DateTimeOffset? StartedAt,
    [property: JsonPropertyName("endedAt")]
    DateTimeOffset? EndedAt,
    [property: JsonPropertyName("callOrder")]
    int CallOrder,
    [property: JsonPropertyName("isBookingConfirmationCall")]
    bool IsBookingConfirmationCall,
    [property: JsonPropertyName("summary")]
    string Summary);
