using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

namespace NICK2._0_core.Infrastructure.SmartMoving.Mappers;

public static class SmartMovingOpportunityMapper
{
    public static SmartMovingOpportunity ToSmartMovingOpportunity(
        this SmartMovingOpportunityApiResponse response,
        IReadOnlyCollection<SmartMovingJobDetailsApiResponse> jobs)
    {
        return new SmartMovingOpportunity(
            response.Id,
            response.QuoteNumber,
            response.Status,
            jobs
                .Select(job => new SmartMovingJob(
                    job.Id,
                    job.JobNumber,
                    job.Notes?.CrewNotes))
                .ToArray());
    }
}
