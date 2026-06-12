using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Application.Webhooks.Services;

namespace NICK2._0_core.Application.Webhooks;

/// <inheritdoc />
public sealed class WebHookProcessingService(
    IStatusValidationService statusValidationService,
    IProcessingStateService processingStateService,
    IDataCollectorService dataCollectorService) : IWebHookProcessingService
{
    /// <inheritdoc />
    public async Task ProcessOpportunityStatusChanged(
        ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        var isValidStatus = await statusValidationService.IsValid(
            command.OpportunityStatus,
            cancellationToken);

        if (!isValidStatus)
        {
            return;
        }

        var shouldProcess = await processingStateService.TryStartProcessing(
            command,
            cancellationToken);

        if (!shouldProcess)
        {
            return;
        }

        await dataCollectorService.CollectOpportunityData(
            command.OpportunityId,
            cancellationToken);

        await processingStateService.MarkProcessed(command, cancellationToken);
    }
}
