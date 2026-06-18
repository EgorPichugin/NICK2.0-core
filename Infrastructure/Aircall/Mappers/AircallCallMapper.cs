using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Infrastructure.Aircall.Mappers;

public static class AircallCallMapper
{
    public static AircallCall ToAircallCall(
        this AircallCallSummaryApiResponse call,
        AircallCallTranscriptionApiResponse transcriptionResponse)
    {
        var utterances = transcriptionResponse
            .Transcription?
            .Content?
            .Utterances
            .Select(utterance => new AircallCallUtterance(
                utterance.StartTime,
                utterance.EndTime,
                utterance.Text,
                utterance.ParticipantType,
                utterance.PhoneNumber,
                utterance.UserId))
            .ToArray() ?? [];

        return new AircallCall(
            call.Id,
            call.Direction,
            utterances);
    }
}
