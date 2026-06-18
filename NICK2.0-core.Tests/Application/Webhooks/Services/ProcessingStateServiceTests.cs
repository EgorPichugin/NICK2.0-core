using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Application.Webhooks.Services;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Tests.Application.Webhooks.Services;

public sealed class ProcessingStateServiceTests
{
    [Fact]
    public async Task TryStartProcessing_DelegatesToRepository()
    {
        var repository = new FakeProcessingStateRepository(true);
        var service = new ProcessingStateService(repository);
        var command = CreateCommand();

        var shouldProcess = await service.TryStartProcessing(command, CancellationToken.None);

        Assert.True(shouldProcess);
        Assert.Equal(command.EventType, repository.EventType);
        Assert.Equal(command.OpportunityId, repository.OpportunityId);
        Assert.Equal(EOpportunityStatus.Booked, repository.OpportunityStatus);
    }

    [Fact]
    public async Task MarkProcessed_DelegatesToRepository()
    {
        var repository = new FakeProcessingStateRepository(true);
        var service = new ProcessingStateService(repository);
        var command = CreateCommand();

        await service.MarkProcessed(command, CancellationToken.None);

        Assert.Equal(1, repository.MarkProcessedCallCount);
        Assert.Equal(command.EventType, repository.EventType);
        Assert.Equal(command.OpportunityId, repository.OpportunityId);
        Assert.Equal(EOpportunityStatus.Booked, repository.OpportunityStatus);
    }

    private static ProcessOpportunityStatusChangedCommand CreateCommand()
    {
        return new ProcessOpportunityStatusChangedCommand(
            ProcessOpportunityStatusChangedCommand.ExpectedEventType,
            Guid.NewGuid(),
            (int)EOpportunityStatus.Booked);
    }

    private sealed class FakeProcessingStateRepository(bool shouldStartProcessing)
        : IProcessingStateRepository
    {
        public string? EventType { get; private set; }

        public Guid? OpportunityId { get; private set; }

        public EOpportunityStatus? OpportunityStatus { get; private set; }

        public int MarkProcessedCallCount { get; private set; }

        public Task<bool> TryAddProcessing(
            string eventType,
            Guid opportunityId,
            EOpportunityStatus opportunityStatus,
            CancellationToken cancellationToken)
        {
            EventType = eventType;
            OpportunityId = opportunityId;
            OpportunityStatus = opportunityStatus;

            return Task.FromResult(shouldStartProcessing);
        }

        public Task MarkProcessed(
            string eventType,
            Guid opportunityId,
            EOpportunityStatus opportunityStatus,
            CancellationToken cancellationToken)
        {
            EventType = eventType;
            OpportunityId = opportunityId;
            OpportunityStatus = opportunityStatus;
            MarkProcessedCallCount++;

            return Task.CompletedTask;
        }
    }
}
