using System.Text.Json;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Services;

namespace NICK2._0_core.Application.Webhooks;

/// <inheritdoc />
public sealed class WebHookProcessingService(
    IStatusValidationService statusValidationService,
    IOpportunityStatusSnapshotService opportunityStatusSnapshotService,
    IProcessingStateService processingStateService,
    IDataCollectorService dataCollectorService,
    ILogger<WebHookProcessingService>? logger = null) : IWebHookProcessingService
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> ProcessOpportunityStatusChanged(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        logger?.LogInformation(
            "Processing SmartMoving webhook {EventType} for opportunity {OpportunityId} with status {OpportunityStatus}.",
            command.EventType,
            command.OpportunityId,
            command.OpportunityStatus);

        if (command.EventType != ProcessOpportunityStatusChangedCommand.ExpectedEventType)
        {
            logger?.LogWarning(
                "Rejected SmartMoving webhook for opportunity {OpportunityId} because event type {ActualEventType} does not match expected event type {ExpectedEventType}.",
                command.OpportunityId,
                command.EventType,
                ProcessOpportunityStatusChangedCommand.ExpectedEventType);

            throw new InvalidWebhookEventTypeException(
                command.EventType,
                ProcessOpportunityStatusChangedCommand.ExpectedEventType);
        }

        var isValidStatus = await statusValidationService.IsValid(
            command.OpportunityStatus,
            cancellationToken);

        if (!isValidStatus)
        {
            logger?.LogInformation(
                "Skipping SmartMoving webhook for opportunity {OpportunityId} because status {OpportunityStatus} is not supported.",
                command.OpportunityId,
                command.OpportunityStatus);

            return null;
        }

        var hasTargetStatusTransition = await opportunityStatusSnapshotService.UpdateAndCheckShouldProcess(
            command,
            cancellationToken);

        if (!hasTargetStatusTransition)
        {
            logger?.LogInformation(
                "Skipping SmartMoving webhook for opportunity {OpportunityId} because status {OpportunityStatus} is not a target transition.",
                command.OpportunityId,
                command.OpportunityStatus);

            return null;
        }

        var shouldProcess = await processingStateService.TryStartProcessing(
            command,
            cancellationToken);

        if (!shouldProcess)
        {
            logger?.LogInformation(
                "Skipping duplicate SmartMoving webhook {EventType} for opportunity {OpportunityId} with status {OpportunityStatus}.",
                command.EventType,
                command.OpportunityId,
                command.OpportunityStatus);

            return null;
        }

        var opportunity = await dataCollectorService.CollectOpportunityData(
            command.OpportunityId,
            cancellationToken);

        if (opportunity is null)
        {
            logger?.LogWarning(
                "SmartMoving opportunity {OpportunityId} was not found while processing webhook {EventType}.",
                command.OpportunityId,
                command.EventType);

            return null;
        }

        await processingStateService.MarkProcessed(command, cancellationToken);
        var opportunitySnapshotPath = await SaveOpportunitySnapshot(opportunity, cancellationToken);

        logger?.LogInformation(
            "Processed SmartMoving webhook {EventType} for opportunity {OpportunityId} with status {OpportunityStatus}.",
            command.EventType,
            command.OpportunityId,
            command.OpportunityStatus);
        logger?.LogInformation(
            "Saved SmartMoving opportunity snapshot to {OutputPath}.",
            opportunitySnapshotPath);

        return opportunity;
    }

    private static async Task<string> SaveOpportunitySnapshot(
        SmartMovingOpportunity opportunity,
        CancellationToken cancellationToken)
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            $"smartmoving-opportunity-{opportunity.Id}.json");

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(
            stream,
            opportunity,
            new JsonSerializerOptions
            {
                WriteIndented = true
            },
            cancellationToken);

        return outputPath;
    }
}
