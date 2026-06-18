using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Application.Webhooks.Services;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Tests.Application.Webhooks.Services;

public sealed class OpportunityStatusSnapshotServiceTests
{
    [Fact]
    public async Task UpdateAndCheckShouldProcess_ReturnsTrue_ForOpportunityToBooked()
    {
        var repository = new FakeOpportunityStatusSnapshotRepository();
        var service = new OpportunityStatusSnapshotService(repository);
        var opportunityId = Guid.NewGuid();

        await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Opportunity),
            CancellationToken.None);

        var shouldProcess = await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Booked),
            CancellationToken.None);

        Assert.True(shouldProcess);
    }

    [Fact]
    public async Task UpdateAndCheckShouldProcess_ReturnsTrue_ForLostToBooked()
    {
        var repository = new FakeOpportunityStatusSnapshotRepository();
        var service = new OpportunityStatusSnapshotService(repository);
        var opportunityId = Guid.NewGuid();

        await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Lost),
            CancellationToken.None);

        var shouldProcess = await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Booked),
            CancellationToken.None);

        Assert.True(shouldProcess);
    }

    [Fact]
    public async Task UpdateAndCheckShouldProcess_ReturnsFalse_ForBookedToBooked()
    {
        var repository = new FakeOpportunityStatusSnapshotRepository();
        var service = new OpportunityStatusSnapshotService(repository);
        var opportunityId = Guid.NewGuid();

        await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Booked),
            CancellationToken.None);

        var shouldProcess = await service.UpdateAndCheckShouldProcess(
            CreateCommand(opportunityId, EOpportunityStatus.Booked),
            CancellationToken.None);

        Assert.False(shouldProcess);
    }

    [Fact]
    public async Task UpdateAndCheckShouldProcess_ReturnsFalse_WhenCurrentStatusIsNotBooked()
    {
        var service = new OpportunityStatusSnapshotService(
            new FakeOpportunityStatusSnapshotRepository());

        var shouldProcess = await service.UpdateAndCheckShouldProcess(
            CreateCommand(Guid.NewGuid(), EOpportunityStatus.Lost),
            CancellationToken.None);

        Assert.False(shouldProcess);
    }

    [Fact]
    public async Task UpdateAndCheckShouldProcess_ReturnsFalse_WhenSnapshotDoesNotExistYet()
    {
        var service = new OpportunityStatusSnapshotService(
            new FakeOpportunityStatusSnapshotRepository());

        var shouldProcess = await service.UpdateAndCheckShouldProcess(
            CreateCommand(Guid.NewGuid(), EOpportunityStatus.Booked),
            CancellationToken.None);

        Assert.False(shouldProcess);
    }

    private sealed class FakeOpportunityStatusSnapshotRepository
        : IOpportunityStatusSnapshotRepository
    {
        private readonly Dictionary<Guid, EOpportunityStatus> _statuses = [];

        public Task<EOpportunityStatus?> UpsertStatus(
            Guid opportunityId,
            EOpportunityStatus currentStatus,
            CancellationToken cancellationToken)
        {
            _statuses.TryGetValue(opportunityId, out var previousStatus);
            var hasPreviousStatus = _statuses.ContainsKey(opportunityId);
            _statuses[opportunityId] = currentStatus;

            return Task.FromResult(hasPreviousStatus ? previousStatus : (EOpportunityStatus?)null);
        }
    }

    private static ProcessOpportunityStatusChangedCommand CreateCommand(
        Guid opportunityId,
        EOpportunityStatus status)
    {
        return new ProcessOpportunityStatusChangedCommand(
            ProcessOpportunityStatusChangedCommand.ExpectedEventType,
            opportunityId,
            (int)status);
    }
}
