using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Services;

namespace NICK2._0_core.Application.Webhooks;

/// <inheritdoc />
public sealed class WebHookProcessingService(
    IStatusValidationService statusValidationService,
    IOpportunityStatusSnapshotService opportunityStatusSnapshotService,
    IProcessingStateService processingStateService,
    IDataCollectorService dataCollectorService) : IWebHookProcessingService
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> ProcessOpportunityStatusChanged(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        if (command.EventType != ProcessOpportunityStatusChangedCommand.ExpectedEventType)
        {
            throw new InvalidWebhookEventTypeException(
                command.EventType,
                ProcessOpportunityStatusChangedCommand.ExpectedEventType);
        }

        var isValidStatus = await statusValidationService.IsValid(
            command.OpportunityStatus,
            cancellationToken);

        if (!isValidStatus)
        {
            return null;
        }

        var hasTargetStatusTransition = await opportunityStatusSnapshotService.UpdateAndCheckShouldProcess(
            command,
            cancellationToken);

        if (!hasTargetStatusTransition)
        {
            return null;
        }

        var shouldProcess = await processingStateService.TryStartProcessing(
            command,
            cancellationToken);

        if (!shouldProcess)
        {
            return null;
        }

        var opportunity = await dataCollectorService.CollectOpportunityData(
            command.OpportunityId,
            cancellationToken);

        if (opportunity is null)
        {
            return null;
        }

        await processingStateService.MarkProcessed(command, cancellationToken);

        return opportunity;
    }
}
