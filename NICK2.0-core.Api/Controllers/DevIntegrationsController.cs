using Microsoft.AspNetCore.Mvc;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Developer;
using NICK2._0_core.Application.SmartMoving;

namespace NICK2._0_core.Controllers;

[ApiController]
[Route("api/dev")]
public sealed class DevIntegrationsController(
    ISmartMovingClient smartMovingClient,
    IAircallClient aircallClient,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("smartmoving/opportunities/{opportunityId:guid}")]
    public async Task<IActionResult> GetSmartMovingOpportunity(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var opportunity = await smartMovingClient.GetOpportunityForDeveloper(opportunityId, cancellationToken);

        return opportunity is null
            ? NotFound()
            : Ok(opportunity);
    }

    [HttpGet("aircall/calls/search")]
    public async Task<IActionResult> SearchAircallCalls(
        [FromQuery] string phoneNumber,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var calls = await aircallClient.SearchCalls(phoneNumber, cancellationToken);

        return Ok(calls.Select(call => call.Id).ToArray());
    }

    [HttpGet("aircall/calls/{callId:long}/summary")]
    public async Task<IActionResult> GetAircallCallAiSummary(
        long callId,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var summary = await aircallClient.GetCallAiSummaryForDeveloper(callId, cancellationToken);

        return summary is null
            ? NotFound()
            : Ok(summary.Response);
    }

    [HttpPost("aircall/calls/summaries")]
    public async Task<IActionResult> GetAircallCallAiSummaries(
        [FromBody] DevAircallCallAiSummaryBatchRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var summaries = new List<System.Text.Json.JsonElement>();
        var notFoundCallIds = new List<long>();

        foreach (var callId in (request.CallIds ?? []).Distinct())
        {
            var summary = await aircallClient.GetCallAiSummaryForDeveloper(callId, cancellationToken);

            if (summary is null)
            {
                notFoundCallIds.Add(callId);

                continue;
            }

            summaries.Add(summary.Response);
        }

        return Ok(new DevAircallCallAiSummaryBatchResponse(
            summaries,
            notFoundCallIds));
    }
}
