using System.Text.Json;
using System.Text.Json.Serialization;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Services;
using NICK2._0_core.Domain;

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

        var summaries = await aircallClient.GetCalls(
            opportunity.CustomerPhoneNumber,
            cancellationToken);
        opportunity = opportunity with
        {
            Summaries = summaries
        };

        var opportunitySnapshotPath = await SaveOpportunitySnapshot(opportunity, cancellationToken);
        await processingStateService.MarkProcessed(command, cancellationToken);

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
            CollectedOpportunitySnapshot.From(opportunity),
            new JsonSerializerOptions
            {
                WriteIndented = true
            },
            cancellationToken);

        return outputPath;
    }

    private sealed record CollectedOpportunitySnapshot(
        [property: JsonPropertyName("Opportunity")]
        OpportunitySnapshot Opportunity,
        [property: JsonPropertyName("Jobs")]
        IReadOnlyCollection<JobSnapshot> Jobs,
        [property: JsonPropertyName("Aircall")]
        AircallSnapshot Aircall)
    {
        public static CollectedOpportunitySnapshot From(SmartMovingOpportunity opportunity)
        {
            return new CollectedOpportunitySnapshot(
                new OpportunitySnapshot(
                    opportunity.Id,
                    opportunity.QuoteNumber,
                    opportunity.CustomerPhoneNumber,
                    new OpportunityStatusSnapshot(
                        opportunity.Status,
                        GetOpportunityStatusName(opportunity.Status)),
                    opportunity.CreatedAt,
                    opportunity.BookedAt,
                    opportunity.StatusChangedAt),
                opportunity.Jobs
                    .Select(job => new JobSnapshot(
                        job.Id,
                        job.JobNumber,
                        job.MoveDate,
                        job.ArrivalWindow,
                        job.CrewNotes))
                    .ToArray(),
                new AircallSnapshot(opportunity.Summaries));
        }

        private static string GetOpportunityStatusName(int status)
        {
            return Enum.GetName(typeof(EOpportunityStatus), status) ?? status.ToString();
        }
    }

    private sealed record OpportunitySnapshot(
        [property: JsonPropertyName("id")]
        Guid Id,
        [property: JsonPropertyName("quoteNumber")]
        int QuoteNumber,
        [property: JsonPropertyName("customerPhoneNumber")]
        string CustomerPhoneNumber,
        [property: JsonPropertyName("status")]
        OpportunityStatusSnapshot Status,
        [property: JsonPropertyName("createdAt")]
        DateTimeOffset CreatedAt,
        [property: JsonPropertyName("bookedAt")]
        DateTimeOffset? BookedAt,
        [property: JsonPropertyName("statusChangedAt")]
        DateTimeOffset? StatusChangedAt);

    private sealed record OpportunityStatusSnapshot(
        [property: JsonPropertyName("code")]
        int Code,
        [property: JsonPropertyName("name")]
        string Name);

    private sealed record JobSnapshot(
        [property: JsonPropertyName("id")]
        Guid Id,
        [property: JsonPropertyName("jobNumber")]
        string JobNumber,
        [property: JsonPropertyName("moveDate")]
        DateOnly? MoveDate,
        [property: JsonPropertyName("arrivalWindow")]
        string? ArrivalWindow,
        [property: JsonPropertyName("crewNotes")]
        string? CrewNotes);

    private sealed record AircallSnapshot(
        [property: JsonPropertyName("calls")]
        IReadOnlyCollection<AircallCall> Calls);
}
