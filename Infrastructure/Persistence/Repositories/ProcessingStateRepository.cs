using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence.Repositories;

public sealed class ProcessingStateRepository(
    AppDbContext dbContext,
    ILogger<ProcessingStateRepository> logger)
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

        if (rowsInserted == 0)
        {
            logger.LogDebug(
                "Processing state already exists for webhook {EventType}, opportunity {OpportunityId}, status {OpportunityStatus}.",
                eventType,
                opportunityId,
                opportunityStatus);

            return false;
        }

        logger.LogDebug(
            "Created processing state for webhook {EventType}, opportunity {OpportunityId}, status {OpportunityStatus}.",
            eventType,
            opportunityId,
            opportunityStatus);

        return true;
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
            logger.LogWarning(
                "Could not mark processing state as processed because it was not found. EventType: {EventType}. OpportunityId: {OpportunityId}. OpportunityStatus: {OpportunityStatus}.",
                eventType,
                opportunityId,
                opportunityStatus);

            return;
        }

        processingState.State = ProcessingState.Processed;
        processingState.ProcessedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogDebug(
            "Marked processing state as processed for webhook {EventType}, opportunity {OpportunityId}, status {OpportunityStatus}.",
            eventType,
            opportunityId,
            opportunityStatus);
    }
}
