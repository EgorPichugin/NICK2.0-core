namespace NICK2._0_core.Application.SmartMoving;

public sealed record SmartMovingOpportunity(
    Guid Id,
    int QuoteNumber,
    int Status,
    IReadOnlyCollection<SmartMovingJob> Jobs);

public sealed record SmartMovingJob(
    Guid Id,
    string JobNumber,
    string? CrewNotes);
