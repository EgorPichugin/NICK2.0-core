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
    public void SummaryResponse_DeserializesExpectedFields()
    {
        const string json = """
        {
          "summary": {
            "id": 63485433,
            "call_id": 3557801845,
            "created_at": "2026-03-02T23:53:27.000Z",
            "content": "The conversation is a client inquiry to a moving company."
          }
        }
        """;

        var result = JsonSerializer.Deserialize<AircallCallAiSummaryApiResponse>(json);

        Assert.NotNull(result);
        Assert.Equal(63485433, result.Summary?.Id);
        Assert.Equal(3557801845, result.Summary?.CallId);
        Assert.Equal("The conversation is a client inquiry to a moving company.", result.Summary?.Content);
    }
}
