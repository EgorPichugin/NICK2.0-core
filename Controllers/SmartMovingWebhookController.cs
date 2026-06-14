using Microsoft.AspNetCore.Mvc;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Commands;

namespace NICK2._0_core.Controllers;

[ApiController]
[Route("api/smartmoving/webhook")]
public sealed class SmartMovingWebhookController(
    IWebHookProcessingService webHookProcessingService,
    ILogger<SmartMovingWebhookController> logger) : ControllerBase
{
    private static readonly Guid TestOpportunityId =
        Guid.Parse("3e48d43e-18a5-4d10-b7f3-b4390145fe28");

    [HttpPost("opportunity-status-changed")]
    public async Task<IActionResult> OpportunityStatusChanged(
        [FromBody] ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Remove after SmartMoving webhook testing is complete.
        if (command.OpportunityId != TestOpportunityId)
        {
            logger.LogInformation(
                "Skipping SmartMoving webhook for non-test opportunity {OpportunityId}.",
                command.OpportunityId);

            return Ok(new { received = true });
        }

        try
        {
            await webHookProcessingService.ProcessOpportunityStatusChanged(command, cancellationToken);
        }
        catch (InvalidWebhookEventTypeException exception)
        {
            return BadRequest(new
            {
                error = exception.Message,
                actualEventType = exception.ActualEventType,
                expectedEventType = exception.ExpectedEventType
            });
        }
        catch (SmartMovingApiException exception)
        {
            logger.LogWarning(
                exception,
                "SmartMoving API request failed while processing webhook. StatusCode: {StatusCode}. Body: {ResponseBody}",
                exception.StatusCode,
                exception.ResponseBody);

            return Ok(new { received = true });
        }

        return Ok(new { received = true });
    }
}
