using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence.Repositories;

public sealed class OpportunityStatusSnapshotRepository(
    AppDbContext dbContext,
    ILogger<OpportunityStatusSnapshotRepository> logger)
    : IOpportunityStatusSnapshotRepository
{
    public async Task<EOpportunityStatus?> UpsertStatus(
        Guid opportunityId,
        EOpportunityStatus currentStatus,
        CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.OpportunityStatusSnapshots
            .SingleOrDefaultAsync(
                item => item.Id == opportunityId,
                cancellationToken);

        var previousStatus = snapshot?.CurrentStatus;

        if (snapshot is null)
        {
            logger.LogDebug(
                "Creating opportunity status snapshot for {OpportunityId} with status {CurrentStatus}.",
                opportunityId,
                currentStatus);

            dbContext.OpportunityStatusSnapshots.Add(new OpportunityStatusSnapshot
            {
                Id = opportunityId,
                CurrentStatus = currentStatus,
                PreviousStatus = null,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            logger.LogDebug(
                "Updating opportunity status snapshot for {OpportunityId} from {PreviousStatus} to {CurrentStatus}.",
                opportunityId,
                snapshot.CurrentStatus,
                currentStatus);

            snapshot.PreviousStatus = snapshot.CurrentStatus;
            snapshot.CurrentStatus = currentStatus;
            snapshot.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return previousStatus;
    }
}
