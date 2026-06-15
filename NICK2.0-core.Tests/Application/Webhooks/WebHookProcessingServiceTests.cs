using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Services;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Tests.Application.Webhooks;

public sealed class WebHookProcessingServiceTests
{
    [Fact]
    public async Task ProcessOpportunityStatusChanged_Throws_WhenEventTypeIsInvalid()
    {
        var service = CreateService();
        var command = CreateCommand(eventType: "wrong-event");

        await Assert.ThrowsAsync<InvalidWebhookEventTypeException>(
            () => service.ProcessOpportunityStatusChanged(command, CancellationToken.None));
    }

    [Fact]
    public async Task ProcessOpportunityStatusChanged_DoesNotCollectData_WhenStatusIsInvalid()
    {
        var dataCollector = new FakeDataCollectorService();
        var service = CreateService(
            statusValidationService: new FakeStatusValidationService(false),
            dataCollectorService: dataCollector);

        var result = await service.ProcessOpportunityStatusChanged(
            CreateCommand(opportunityStatus: 999),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, dataCollector.CollectCallCount);
    }

    [Fact]
    public async Task ProcessOpportunityStatusChanged_DoesNotCollectData_WhenEventAlreadyStarted()
    {
        var dataCollector = new FakeDataCollectorService();
        var processingState = new FakeProcessingStateService(false);
        var service = CreateService(
            processingStateService: processingState,
            dataCollectorService: dataCollector);

        var result = await service.ProcessOpportunityStatusChanged(
            CreateCommand(),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(1, processingState.TryStartProcessingCallCount);
        Assert.Equal(0, dataCollector.CollectCallCount);
        Assert.Equal(0, processingState.MarkProcessedCallCount);
    }

    [Fact]
    public async Task ProcessOpportunityStatusChanged_DoesNotCollectData_WhenStatusTransitionIsNotTarget()
    {
        var dataCollector = new FakeDataCollectorService();
        var processingState = new FakeProcessingStateService(true);
        var service = CreateService(
            opportunityStatusSnapshotService: new FakeOpportunityStatusSnapshotService(false),
            processingStateService: processingState,
            dataCollectorService: dataCollector);

        var result = await service.ProcessOpportunityStatusChanged(
            CreateCommand(opportunityStatus: (int)OpportunityStatus.Booked),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, processingState.TryStartProcessingCallCount);
        Assert.Equal(0, dataCollector.CollectCallCount);
        Assert.Equal(0, processingState.MarkProcessedCallCount);
    }

    [Fact]
    public async Task ProcessOpportunityStatusChanged_DoesNotMarkProcessed_WhenOpportunityIsNotFound()
    {
        var processingState = new FakeProcessingStateService(true);
        var service = CreateService(
            opportunityStatusSnapshotService: new FakeOpportunityStatusSnapshotService(true),
            processingStateService: processingState,
            dataCollectorService: new FakeDataCollectorService());

        var result = await service.ProcessOpportunityStatusChanged(
            CreateCommand(opportunityStatus: (int)OpportunityStatus.Booked),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(1, processingState.TryStartProcessingCallCount);
        Assert.Equal(0, processingState.MarkProcessedCallCount);
    }

    [Fact]
    public async Task ProcessOpportunityStatusChanged_ReturnsOpportunityAndMarksProcessed_WhenEventIsValid()
    {
        var expectedOpportunity = new SmartMovingOpportunity(Guid.NewGuid(), 90690, 3);
        var dataCollector = new FakeDataCollectorService(expectedOpportunity);
        var processingState = new FakeProcessingStateService(true);
        var service = CreateService(
            opportunityStatusSnapshotService: new FakeOpportunityStatusSnapshotService(true),
            processingStateService: processingState,
            dataCollectorService: dataCollector);

        var result = await service.ProcessOpportunityStatusChanged(
            CreateCommand(
                opportunityId: expectedOpportunity.Id,
                opportunityStatus: (int)OpportunityStatus.Booked),
            CancellationToken.None);

        Assert.Equal(expectedOpportunity, result);
        Assert.Equal(1, dataCollector.CollectCallCount);
        Assert.Equal(1, processingState.MarkProcessedCallCount);
    }

    private static WebHookProcessingService CreateService(
        IStatusValidationService? statusValidationService = null,
        IOpportunityStatusSnapshotService? opportunityStatusSnapshotService = null,
        IProcessingStateService? processingStateService = null,
        IDataCollectorService? dataCollectorService = null)
    {
        return new WebHookProcessingService(
            statusValidationService ?? new FakeStatusValidationService(true),
            opportunityStatusSnapshotService ?? new FakeOpportunityStatusSnapshotService(true),
            processingStateService ?? new FakeProcessingStateService(true),
            dataCollectorService ?? new FakeDataCollectorService());
    }

    private static ProcessOpportunityStatusChangedCommand CreateCommand(
        string eventType = ProcessOpportunityStatusChangedCommand.ExpectedEventType,
        Guid? opportunityId = null,
        int opportunityStatus = 3)
    {
        return new ProcessOpportunityStatusChangedCommand(
            eventType,
            opportunityId ?? Guid.NewGuid(),
            opportunityStatus);
    }

    private sealed class FakeStatusValidationService(bool isValid) : IStatusValidationService
    {
        public Task<bool> IsValid(int opportunityStatus, CancellationToken cancellationToken)
        {
            return Task.FromResult(isValid);
        }
    }

    private sealed class FakeProcessingStateService(bool shouldStartProcessing) : IProcessingStateService
    {
        public int TryStartProcessingCallCount { get; private set; }

        public int MarkProcessedCallCount { get; private set; }

        public Task<bool> TryStartProcessing(
            ProcessOpportunityStatusChangedCommand command,
            CancellationToken cancellationToken)
        {
            TryStartProcessingCallCount++;

            return Task.FromResult(shouldStartProcessing);
        }

        public Task MarkProcessed(
            ProcessOpportunityStatusChangedCommand command,
            CancellationToken cancellationToken)
        {
            MarkProcessedCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeOpportunityStatusSnapshotService(bool shouldProcess)
        : IOpportunityStatusSnapshotService
    {
        public Task<bool> UpdateAndCheckShouldProcess(
            ProcessOpportunityStatusChangedCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(shouldProcess);
        }
    }

    private sealed class FakeDataCollectorService(
        SmartMovingOpportunity? opportunity = null) : IDataCollectorService
    {
        public int CollectCallCount { get; private set; }

        public Task<SmartMovingOpportunity?> CollectOpportunityData(
            Guid opportunityId,
            CancellationToken cancellationToken)
        {
            CollectCallCount++;

            return Task.FromResult(opportunity);
        }
    }
}
