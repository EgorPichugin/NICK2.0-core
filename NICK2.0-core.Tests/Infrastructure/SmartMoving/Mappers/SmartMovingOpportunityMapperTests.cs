using NICK2._0_core.Infrastructure.SmartMoving.Mappers;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

namespace NICK2._0_core.Tests.Infrastructure.SmartMoving.Mappers;

public sealed class SmartMovingOpportunityMapperTests
{
    [Fact]
    public void ToSmartMovingOpportunity_MapsExpectedFields()
    {
        var id = Guid.NewGuid();
        var response = new SmartMovingOpportunityApiResponse(
            id,
            90690,
            new SmartMovingCustomerApiResponse(Guid.NewGuid(), "Andrei Sid", "andrei@example.com", "7472874522", 0),
            new SmartMovingBranchApiResponse("Royal Moving LA", "(424) 500-2221"),
            [],
            0,
            1,
            20260611,
            3,
            null,
            new SmartMovingMoveSizeApiResponse("Room or Less", string.Empty, 0),
            0,
            0,
            0,
            new SmartMovingEstimatedTotalApiResponse(906, 0, 0, 906),
            null,
            new SmartMovingSalesAssigneeApiResponse(Guid.NewGuid(), "Andrei"),
            false,
            "Brick + Timber",
            null,
            null,
            true,
            null,
            null,
            null,
            null,
            [],
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new SmartMovingTariffApiResponse(Guid.NewGuid(), "RMC Los Angeles"),
            DateTimeOffset.Parse("2026-04-27T19:46:54.2085361+00:00"));

        var jobId = Guid.NewGuid();
        var result = response.ToSmartMovingOpportunity(
            [
                new SmartMovingJobDetailsApiResponse(
                    jobId,
                    "JOB-1",
                    new SmartMovingJobNotesApiResponse(
                        "Crew notes",
                        "Customer notes",
                        "Internal notes",
                        "Crew feedback",
                        "Accounting notes",
                        "Dispatcher notes"))
            ]);

        Assert.Equal(id, result.Id);
        Assert.Equal(90690, result.QuoteNumber);
        Assert.Equal("7472874522", result.CustomerPhoneNumber);
        Assert.Equal(3, result.Status);
        var job = Assert.Single(result.Jobs);
        Assert.Equal(jobId, job.Id);
        Assert.Equal("JOB-1", job.JobNumber);
        Assert.Equal("Crew notes", job.CrewNotes);
    }
}
