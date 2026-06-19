using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Infrastructure.Aircall.Mappers;

public static class AircallCallMapper
{
    public static AircallCall ToAircallCall(
        this AircallCallSummaryApiResponse call,
        AircallCallAiSummaryApiResponse summaryResponse,
        int callOrder)
    {
        var summary = summaryResponse.Summary?.Content.Trim() ?? string.Empty;

        return new AircallCall(
            call.Id,
            call.Direction,
            ToDateTimeOffset(call.StartedAt),
            ToDateTimeOffset(call.EndedAt),
            callOrder,
            IsBookingConfirmationSummary(summary),
            summary);
    }

    private static DateTimeOffset? ToDateTimeOffset(long? unixTimestamp)
    {
        return unixTimestamp.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(unixTimestamp.Value)
            : null;
    }

    private static bool IsBookingConfirmationSummary(string summary)
    {
        return summary.Contains("confirmed and booked", StringComparison.OrdinalIgnoreCase)
            || summary.Contains("confirmed and scheduled", StringComparison.OrdinalIgnoreCase)
            || summary.Contains("booked a move", StringComparison.OrdinalIgnoreCase);
    }
}
