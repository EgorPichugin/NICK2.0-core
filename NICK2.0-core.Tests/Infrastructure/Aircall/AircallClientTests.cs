using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NICK2._0_core.Infrastructure.Aircall;

namespace NICK2._0_core.Tests.Infrastructure.Aircall;

public sealed class AircallClientTests
{
    [Fact]
    public async Task SearchCalls_ReturnsCallsWithoutAiSummaries()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            var path = request.RequestUri?.AbsolutePath;

            return path switch
            {
                "/v1/calls/search" => CreateJsonResponse("""
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
                          "direct_link": "https://dashboard.aircall.io/calls/812",
                          "direction": "outbound",
                          "status": "done",
                          "started_at": 1781807700,
                          "answered_at": 1781807720,
                          "ended_at": 1781808120,
                          "duration": 420,
                          "raw_digits": "+1 800-123-4567"
                        }
                      ]
                    }
                    """),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });
        var client = CreateClient(handler);

        var result = await client.SearchCalls("8001234567", CancellationToken.None);

        var call = Assert.Single(result);
        Assert.Equal(812, call.Id);
        Assert.Equal("https://dashboard.aircall.io/calls/812", call.DirectLink);
        Assert.Equal("outbound", call.Direction);
        Assert.Equal("done", call.Status);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781807700), call.StartedAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781807720), call.AnsweredAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781808120), call.EndedAt);
        Assert.Equal(420, call.Duration);
        Assert.Equal("+1 800-123-4567", call.RawDigits);
        Assert.DoesNotContain(
            handler.Requests,
            request => request.RequestUri?.AbsolutePath.EndsWith("/summary") == true);
    }

    [Fact]
    public async Task GetCallAiSummary_ReturnsSummary()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            var path = request.RequestUri?.AbsolutePath;

            return path switch
            {
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
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });
        var client = CreateClient(handler);

        var result = await client.GetCallAiSummary(812, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(63485433, result.Id);
        Assert.Equal(812, result.CallId);
        Assert.Equal(DateTimeOffset.Parse("2026-03-02T23:53:27.000Z"), result.CreatedAt);
        Assert.Equal("Client asks for a moving estimate.", result.Content);
    }

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
        var client = CreateClient(handler);

        var callsFrom = DateTimeOffset.Parse("2026-06-17T00:00:00Z");

        var result = await client.GetCalls("8001234567", callsFrom, CancellationToken.None);

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
            request => request.RequestUri?.PathAndQuery == "/v1/calls/search?phone_number=%2b18001234567&from=1781654400");
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    private static AircallClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.aircall.io/v1/")
        };

        return new AircallClient(
            httpClient,
            Options.Create(new AircallOptions()),
            NullLogger<AircallClient>.Instance);
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
