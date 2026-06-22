using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Commands;
using NICK2._0_core.Infrastructure.SmartMoving;

namespace NICK2._0_core.Controllers;

[ApiController]
[Route("api/smartmoving/webhook")]
public sealed class SmartMovingWebhookController(
    IWebHookProcessingService webHookProcessingService,
    IOptions<SmartMovingOptions> smartMovingOptions,
    IConfiguration configuration,
    ILogger<SmartMovingWebhookController> logger) : ControllerBase
{
    private const string WebhookTokenHeaderName = "X-SmartMoving-Webhook-Token";

    private static readonly Guid TestOpportunityId =
        Guid.Parse("578beceb-0553-4ef3-85d6-b40101895952");

    [HttpPost("opportunity-status-changed")]
    public async Task<IActionResult> OpportunityStatusChanged(
        [FromBody] ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedWebhookRequest())
        {
            logger.LogWarning("Rejected SmartMoving webhook request with invalid token.");

            return Unauthorized();
        }

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

    private bool IsAuthorizedWebhookRequest()
    {
        var expectedToken = smartMovingOptions.Value.WebhookToken;

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            expectedToken = configuration["SmartMoving_WebhookToken"] ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            return false;
        }

        return Request.Headers.TryGetValue(WebhookTokenHeaderName, out var actualToken)
            && actualToken == expectedToken;
    }
}
