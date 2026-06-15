using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        dbContext.ProcessingStates.Add(new ProcessingStateRecord
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            OpportunityId = opportunityId,
            OpportunityStatus = opportunityStatus,
            State = ProcessingState.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            return false;
        }
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
