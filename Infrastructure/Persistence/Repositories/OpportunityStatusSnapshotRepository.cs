using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence.Repositories;

public sealed class OpportunityStatusSnapshotRepository(AppDbContext dbContext)
    : IOpportunityStatusSnapshotRepository
{
    public async Task<OpportunityStatus?> UpsertStatus(
        Guid opportunityId,
        OpportunityStatus currentStatus,
        CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.OpportunityStatusSnapshots
            .SingleOrDefaultAsync(
                item => item.Id == opportunityId,
                cancellationToken);

        var previousStatus = snapshot?.CurrentStatus;

        if (snapshot is null)
        {
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
            snapshot.PreviousStatus = snapshot.CurrentStatus;
            snapshot.CurrentStatus = currentStatus;
            snapshot.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return previousStatus;
    }
}
