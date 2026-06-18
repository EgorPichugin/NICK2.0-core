using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Infrastructure.Aircall.Mappers;

public static class AircallCallMapper
{
    public static AircallCall ToAircallCall(
        this AircallCallSummaryApiResponse call,
        AircallCallAiSummaryApiResponse summaryResponse)
    {
        return new AircallCall(
            call.Id,
            call.Direction,
            summaryResponse.Summary?.Content.Trim() ?? string.Empty);
    }
}
