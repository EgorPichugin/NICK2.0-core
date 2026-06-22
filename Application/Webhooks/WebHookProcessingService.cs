using System.Text.Json;
using Microsoft.Extensions.Logging;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.Processing;
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
    ILlmService llmService,
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

        var aircallCalls = await aircallClient.GetCalls(
            opportunity.CustomerPhoneNumber,
            opportunity.CreatedAt.AddDays(-1),
            cancellationToken);
        var processingData = new OpportunityProcessingData(
            opportunity,
            aircallCalls,
            OpportunityFactsCatalog.GetFacts(opportunity.Type));

        processingData = await llmService.AnalyzeOpportunity(processingData, cancellationToken);
        logger?.LogInformation(
            "Received LLM analysis for SmartMoving opportunity {OpportunityId}. IsMocked: {IsMocked}.",
            processingData.AnalysisResult?.OpportunityId,
            processingData.AnalysisResult?.IsMocked);

        // var opportunitySnapshotPath = await SaveOpportunitySnapshot(processingData, cancellationToken);
        await processingStateService.MarkProcessed(command, cancellationToken);

        logger?.LogInformation(
            "Processed SmartMoving webhook {EventType} for opportunity {OpportunityId} with status {OpportunityStatus}.",
            command.EventType,
            command.OpportunityId,
            command.OpportunityStatus);
        // logger?.LogInformation(
        //     "Saved SmartMoving opportunity snapshot to {OutputPath}.",
        //     opportunitySnapshotPath);

        return opportunity;
    }

    /// <summary>
    /// Saves the collected SmartMoving opportunity data to a JSON snapshot file in the output directory.
    /// </summary>
    /// <param name="opportunity">The SmartMoving opportunity data to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The full path to the created snapshot file.</returns>
    private static async Task<string> SaveOpportunitySnapshot(
        OpportunityProcessingData processingData,
        CancellationToken cancellationToken)
    {
        var opportunity = processingData.Opportunity;
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            $"smartmoving-opportunity-{opportunity.Id}.json");

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(
            stream,
            processingData,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            },
            cancellationToken);

        return outputPath;
    }
}
