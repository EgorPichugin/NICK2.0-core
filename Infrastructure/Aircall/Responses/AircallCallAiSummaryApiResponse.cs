using System.Text.Json.Serialization;

namespace NICK2._0_core.Infrastructure.Aircall.Responses;

public sealed record AircallCallAiSummaryApiResponse
{
    [JsonPropertyName("summary")]
    public AircallSummaryApiResponse? Summary { get; init; }
}

public sealed record AircallSummaryApiResponse
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("call_id")]
    public long CallId { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}
