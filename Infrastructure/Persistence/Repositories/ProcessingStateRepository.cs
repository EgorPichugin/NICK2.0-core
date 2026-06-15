using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence.Repositories;

public sealed class ProcessingStateRepository(AppDbContext dbContext)
    : IProcessingStateRepository
{
    public async Task<bool> TryAddProcessing(
        string eventType,
        Guid opportunityId,
        OpportunityStatus opportunityStatus,
        CancellationToken cancellationToken)
    {
        var rowsInserted = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO processing_states ("Id", "EventType", "OpportunityId", "OpportunityStatus", "State", "CreatedAt")
            VALUES ({Guid.NewGuid()}, {eventType}, {opportunityId}, {(int)opportunityStatus}, {(int)ProcessingState.Processing}, {DateTimeOffset.UtcNow})
            ON CONFLICT ("EventType", "OpportunityId", "OpportunityStatus") DO NOTHING
            """, cancellationToken);

        return rowsInserted > 0;
    }

    public async Task MarkProcessed(
        string eventType,
        Guid opportunityId,
        OpportunityStatus opportunityStatus,
        CancellationToken cancellationToken)
    {
        var processingState = await dbContext.ProcessingStates
            .SingleOrDefaultAsync(
                item => item.EventType == eventType
                    && item.OpportunityId == opportunityId
                    && item.OpportunityStatus == opportunityStatus,
                cancellationToken);

        if (processingState is null)
        {
            return;
        }

        processingState.State = ProcessingState.Processed;
        processingState.ProcessedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
