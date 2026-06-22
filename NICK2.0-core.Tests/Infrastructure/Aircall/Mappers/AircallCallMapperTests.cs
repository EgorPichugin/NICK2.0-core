using NICK2._0_core.Infrastructure.Aircall.Mappers;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Tests.Infrastructure.Aircall.Mappers;

public sealed class AircallCallMapperTests
{
    [Fact]
    public void ToAircallCall_MapsExpectedFields()
    {
        var call = new AircallCallSummaryApiResponse
        {
            Id = 812,
            Direction = "outbound",
            StartedAt = 1781807700,
            EndedAt = 1781808120
        };
        var summary = new AircallCallAiSummaryApiResponse
        {
            Summary = new AircallSummaryApiResponse
            {
                Id = 63485433,
                CallId = 812,
                Content = " Client asked about a long-distance move estimate. "
            }
        };

        var result = call.ToAircallCall(summary, 6);

        Assert.Equal(812, result.Id);
        Assert.Equal("outbound", result.Direction);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781807700), result.StartedAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1781808120), result.EndedAt);
        Assert.Equal(6, result.CallOrder);
        Assert.False(result.IsBookingConfirmationCall);
        Assert.Equal("Client asked about a long-distance move estimate.", result.Summary);
    }
}
