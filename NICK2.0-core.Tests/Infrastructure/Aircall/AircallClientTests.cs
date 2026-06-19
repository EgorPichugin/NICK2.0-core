using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NICK2._0_core.Infrastructure.Aircall;

namespace NICK2._0_core.Tests.Infrastructure.Aircall;

public sealed class AircallClientTests
{
    [Fact]
    public async Task GetCalls_ReturnsCallsWithAiSummaries()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            var path = request.RequestUri?.AbsolutePath;

            return path switch
            {
                "/v1/calls/search" => CreateJsonResponse("""
                    {
                      "meta": {
                        "count": 2,
                        "total": 2,
                        "current_page": 1,
                        "per_page": 20,
                        "next_page_link": null,
                        "previous_page_link": null
                      },
                      "calls": [
                        { "id": 812, "direction": "outbound", "started_at": 1781807700, "ended_at": 1781808120, "raw_digits": "+1 800-123-4567" },
                        { "id": 813, "direction": "inbound", "started_at": 1781810000, "ended_at": 1781810300, "raw_digits": "+1 800-123-4567" }
                      ]
                    }
                    """),
                "/v1/calls/812/summary" => CreateJsonResponse("""
                    {
                      "summary": {
                        "id": 63485433,
                        "call_id": 812,
                        "created_at": "2026-03-02T23:53:27.000Z",
                        "content": "Client asks for a moving estimate."
                      }
                    }
                    """),
                "/v1/calls/813/summary" => CreateJsonResponse("""
                    {
                      "summary": {
                        "id": 63485434,
                        "call_id": 813,
                        "created_at": "2026-03-02T23:54:27.000Z",
                        "content": "Customer confirmed and booked a move."
                      }
                    }
                    """),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.aircall.io/v1/")
        };
        var client = new AircallClient(
            httpClient,
            Options.Create(new AircallOptions()),
            NullLogger<AircallClient>.Instance);

        var result = await client.GetCalls("8001234567", CancellationToken.None);

        Assert.Collection(
            result,
            call =>
            {
                Assert.Equal(812, call.Id);
                Assert.Equal("outbound", call.Direction);
                Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781807700), call.StartedAt);
                Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781808120), call.EndedAt);
                Assert.Equal(1, call.CallOrder);
                Assert.False(call.IsBookingConfirmationCall);
                Assert.Equal("Client asks for a moving estimate.", call.Summary);
            },
            call =>
            {
                Assert.Equal(813, call.Id);
                Assert.Equal("inbound", call.Direction);
                Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781810000), call.StartedAt);
                Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781810300), call.EndedAt);
                Assert.Equal(2, call.CallOrder);
                Assert.True(call.IsBookingConfirmationCall);
                Assert.Equal("Customer confirmed and booked a move.", call.Summary);
            });
        Assert.Contains(
            handler.Requests,
            request => request.RequestUri?.PathAndQuery == "/v1/calls/search?phone_number=%2b18001234567");
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(send(request));
        }
    }
}
