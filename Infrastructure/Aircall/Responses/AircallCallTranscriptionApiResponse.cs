using System.Text.Json.Serialization;

namespace NICK2._0_core.Infrastructure.Aircall.Responses;

public sealed record AircallCallTranscriptionApiResponse
{
    [JsonPropertyName("transcription")]
    public AircallTranscriptionApiResponse? Transcription { get; init; }
}

public sealed record AircallTranscriptionApiResponse
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("call_id")]
    public long CallId { get; init; }

    [JsonPropertyName("call_created_at")]
    public DateTimeOffset? CallCreatedAt { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("content")]
    public AircallTranscriptionContentApiResponse? Content { get; init; }
}

public sealed record AircallTranscriptionContentApiResponse
{
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("utterances")]
    public IReadOnlyCollection<AircallTranscriptionUtteranceApiResponse> Utterances { get; init; } = [];
}

public sealed record AircallTranscriptionUtteranceApiResponse
{
    [JsonPropertyName("start_time")]
    public decimal StartTime { get; init; }

    [JsonPropertyName("end_time")]
    public decimal EndTime { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("participant_type")]
    public string ParticipantType { get; init; } = string.Empty;

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; init; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; init; }
}
