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
            Direction = "outbound"
        };
        var transcription = new AircallCallTranscriptionApiResponse
        {
            Transcription = new AircallTranscriptionApiResponse
            {
                CallId = 812,
                Content = new AircallTranscriptionContentApiResponse
                {
                    Utterances =
                    [
                        new AircallTranscriptionUtteranceApiResponse
                        {
                            StartTime = 12.54m,
                            EndTime = 13.8m,
                            Text = "Okay,",
                            ParticipantType = "external",
                            PhoneNumber = "+33679198915"
                        },
                        new AircallTranscriptionUtteranceApiResponse
                        {
                            StartTime = 238.08m,
                            EndTime = 239.48m,
                            Text = "Okay, I guess that's enough.",
                            ParticipantType = "internal",
                            UserId = 123
                        }
                    ]
                }
            }
        };

        var result = call.ToAircallCall(transcription);

        Assert.Equal(812, result.Id);
        Assert.Equal("outbound", result.Direction);
        Assert.Collection(
            result.Utterances,
            utterance =>
            {
                Assert.Equal("Okay,", utterance.Text);
                Assert.Equal("external", utterance.ParticipantType);
                Assert.Equal("+33679198915", utterance.PhoneNumber);
                Assert.Null(utterance.UserId);
            },
            utterance =>
            {
                Assert.Equal("Okay, I guess that's enough.", utterance.Text);
                Assert.Equal("internal", utterance.ParticipantType);
                Assert.Null(utterance.PhoneNumber);
                Assert.Equal(123, utterance.UserId);
            });
    }
}
