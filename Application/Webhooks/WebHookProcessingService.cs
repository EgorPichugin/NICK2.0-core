using System.Text.Json;
using NICK2._0_core.Application.Aircall;
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
    IAircallClient aircallClient,
    ILogger<WebHookProcessingService>? logger = null) : IWebHookProcessingService
{
    /// <summary>
    /// Processes a SmartMoving opportunity status webhook, collects matching SmartMoving and Aircall data,
    /// and saves the collected snapshots when the webhook should be handled.
    /// </summary>
    /// <param name="command">The webhook command containing the opportunity status change payload.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    /// The collected SmartMoving opportunity when processing succeeds; otherwise, <see langword="null" />
    /// when the webhook is skipped or the opportunity cannot be found.
    /// </returns>
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

        var calls = await aircallClient.GetCalls(
            opportunity.CustomerPhoneNumber,
            cancellationToken);

        var opportunitySnapshotPath = await SaveOpportunitySnapshot(opportunity, cancellationToken);
        var aircallCallsSnapshotPath = await SaveAircallCallsSnapshot(
            opportunity.Id,
            calls,
            cancellationToken);
        await processingStateService.MarkProcessed(command, cancellationToken);

        logger?.LogInformation(
            "Processed SmartMoving webhook {EventType} for opportunity {OpportunityId} with status {OpportunityStatus}.",
            command.EventType,
            command.OpportunityId,
            command.OpportunityStatus);
        logger?.LogInformation(
            "Saved SmartMoving opportunity snapshot to {OutputPath}.",
            opportunitySnapshotPath);
        logger?.LogInformation(
            "Saved Aircall calls snapshot to {OutputPath}.",
            aircallCallsSnapshotPath);

        return opportunity;
    }

    /// <summary>
    /// Saves the collected SmartMoving opportunity data to a JSON snapshot file in the output directory.
    /// </summary>
    /// <param name="opportunity">The SmartMoving opportunity data to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The full path to the created snapshot file.</returns>
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

    /// <summary>
    /// Saves the Aircall calls collected for an opportunity to a JSON snapshot file in the output directory.
    /// </summary>
    /// <param name="opportunityId">The SmartMoving opportunity identifier used to name the snapshot file.</param>
    /// <param name="calls">The Aircall calls to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The full path to the created snapshot file.</returns>
    private static async Task<string> SaveAircallCallsSnapshot(
        Guid opportunityId,
        IReadOnlyCollection<AircallCall> calls,
        CancellationToken cancellationToken)
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            $"aircall-calls-{opportunityId}.json");

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(
            stream,
            calls,
            new JsonSerializerOptions
            {
                WriteIndented = true
            },
            cancellationToken);

        return outputPath;
    }
}
