using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.SmartMoving;

public sealed record SmartMovingOpportunity(
    Guid Id,
    int QuoteNumber,
    string CustomerPhoneNumber,
    EType Type,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? BookedAt,
    DateTimeOffset? StatusChangedAt,
    IReadOnlyCollection<SmartMovingJob> Jobs,
    IReadOnlyCollection<AircallCall> Summaries);

public sealed record SmartMovingJob(
    Guid Id,
    string JobNumber,
    DateOnly? MoveDate,
    string? ArrivalWindow,
    string? CrewNotes);
