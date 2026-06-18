namespace NICK2._0_core.Application.Aircall;

public sealed record AircallCall(
    long Id,
    string Direction,
    IReadOnlyCollection<AircallCallUtterance> Utterances);

public sealed record AircallCallUtterance(
    decimal StartTime,
    decimal EndTime,
    string Text,
    string ParticipantType,
    string? PhoneNumber,
    long? UserId);
