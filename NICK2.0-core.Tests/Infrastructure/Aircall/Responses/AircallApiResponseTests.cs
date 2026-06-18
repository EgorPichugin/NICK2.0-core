using System.Text.Json;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Tests.Infrastructure.Aircall.Responses;

public sealed class AircallApiResponseTests
{
    [Fact]
    public void CallsResponse_DeserializesExpectedFields()
    {
        const string json = """
        {
          "meta": {
            "count": 1,
            "total": 1,
            "current_page": 1,
            "per_page": 20,
            "next_page_link": null,
            "previous_page_link": null
          },
          "calls": [
            {
              "id": 812,
              "direct_link": "https://api.aircall.io/v1/calls/812",
              "direction": "outbound",
              "status": "done",
              "started_at": 1584998199,
              "answered_at": 1584998205,
              "ended_at": 1584998210,
              "duration": 11,
              "raw_digits": "+1 800-123-4567"
            }
          ]
        }
        """;

        var result = JsonSerializer.Deserialize<AircallCallsApiResponse>(json);

        Assert.NotNull(result);
        Assert.Equal(1, result.Meta?.Count);
        var call = Assert.Single(result.Calls);
        Assert.Equal(812, call.Id);
        Assert.Equal("outbound", call.Direction);
        Assert.Equal("+1 800-123-4567", call.RawDigits);
    }

    [Fact]
    public void TranscriptionResponse_DeserializesExpectedFields()
    {
        const string json = """
        {
          "transcription": {
            "id": 68271,
            "call_id": 5237603,
            "call_created_at": "2024-07-26T13:30:54.000Z",
            "type": "call",
            "content": {
              "language": "en",
              "utterances": [
                {
                  "start_time": 12.54,
                  "end_time": 13.8,
                  "text": "Okay,",
                  "participant_type": "external",
                  "phone_number": "+33679198915"
                },
                {
                  "start_time": 238.08,
                  "end_time": 239.48,
                  "text": "Okay, I guess that's enough.",
                  "participant_type": "internal",
                  "user_id": 123
                }
              ]
            }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<AircallCallTranscriptionApiResponse>(json);

        Assert.NotNull(result);
        Assert.Equal(5237603, result.Transcription?.CallId);
        Assert.Equal("en", result.Transcription?.Content?.Language);
        Assert.Collection(
            result.Transcription?.Content?.Utterances ?? [],
            utterance =>
            {
                Assert.Equal("external", utterance.ParticipantType);
                Assert.Equal("+33679198915", utterance.PhoneNumber);
            },
            utterance =>
            {
                Assert.Equal("internal", utterance.ParticipantType);
                Assert.Equal(123, utterance.UserId);
            });
    }
}
