namespace NICK2._0_core.Infrastructure.SmartMoving.Responses;

public sealed record SmartMovingJobDetailsApiResponse(
    Guid Id,
    string JobNumber,
    SmartMovingJobNotesApiResponse? Notes);

public sealed record SmartMovingJobNotesApiResponse(
    string? CrewNotes,
    string? CustomerNotes,
    string? InternalNotes,
    string? CrewFeedback,
    string? AccountingNotes,
    string? DispatcherNotes);
