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
    [HttpPost("opportunity-status-changed")]
    public async Task<IActionResult> OpportunityStatusChanged(
        [FromBody] ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
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
