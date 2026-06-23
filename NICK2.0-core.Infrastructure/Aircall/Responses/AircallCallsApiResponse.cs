using System.Text.Json.Serialization;

namespace NICK2._0_core.Infrastructure.Aircall.Responses;

public sealed record AircallCallsApiResponse
{
    [JsonPropertyName("meta")]
    public AircallCallsMetaApiResponse? Meta { get; init; }

    [JsonPropertyName("calls")]
    public IReadOnlyCollection<AircallCallSummaryApiResponse> Calls { get; init; } = [];
}

public sealed record AircallCallsMetaApiResponse
{
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("current_page")]
    public int CurrentPage { get; init; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }

    [JsonPropertyName("next_page_link")]
    public string? NextPageLink { get; init; }

    [JsonPropertyName("previous_page_link")]
    public string? PreviousPageLink { get; init; }
}

public sealed record AircallCallSummaryApiResponse
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("direct_link")]
    public string? DirectLink { get; init; }

    [JsonPropertyName("direction")]
    public string Direction { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("started_at")]
    public long? StartedAt { get; init; }

    [JsonPropertyName("answered_at")]
    public long? AnsweredAt { get; init; }

    [JsonPropertyName("ended_at")]
    public long? EndedAt { get; init; }

    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    [JsonPropertyName("raw_digits")]
    public string? RawDigits { get; init; }
}
