using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Domain;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

namespace NICK2._0_core.Infrastructure.SmartMoving.Mappers;

public static class SmartMovingOpportunityMapper
{
    public static SmartMovingOpportunity ToSmartMovingOpportunity(
        this SmartMovingOpportunityApiResponse response,
        IReadOnlyCollection<SmartMovingJobDetailsApiResponse> jobs)
    {
        var jobDetailsById = jobs.ToDictionary(job => job.Id);

        return new SmartMovingOpportunity(
            response.Id,
            response.QuoteNumber,
            response.Customer.PhoneNumber,
            ToOpportunityType(response.Type),
            response.Status,
            response.CreatedAtUtc,
            null,
            null,
            response.Jobs
                .Select(job => new SmartMovingJob(
                    job.Id,
                    job.JobNumber,
                    ToDateOnly(job.JobDate),
                    job.ArrivalWindow.Description,
                    jobDetailsById.GetValueOrDefault(job.Id)?.Notes?.CrewNotes))
                .ToArray(),
            []);
    }

    private static DateOnly? ToDateOnly(int date)
    {
        var text = date.ToString();

        return DateOnly.TryParseExact(text, "yyyyMMdd", out var parsed)
            ? parsed
            : null;
    }

    private static EType ToOpportunityType(int type)
    {
        return Enum.IsDefined(typeof(EType), type)
            ? (EType)type
            : EType.Unknown;
    }
}
