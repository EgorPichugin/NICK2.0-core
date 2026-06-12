using Microsoft.AspNetCore.Mvc;
using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Commands;

namespace NICK2._0_core.Controllers;

[ApiController]
[Route("api/smartmoving/webhook")]
public sealed class SmartMovingWebhookController(
    IWebHookProcessingService webHookProcessingService) : ControllerBase
{
    [HttpPost("opportunity-status-changed")]
    public async Task<IActionResult> OpportunityStatusChanged(
        [FromBody] ProcessOpportunityStatusChangedCommand command,
        CancellationToken cancellationToken)
    {
        await webHookProcessingService.ProcessOpportunityStatusChanged(command, cancellationToken);

        return Ok(new { received = true });
    }
}
