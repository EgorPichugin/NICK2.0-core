using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Infrastructure.Persistence;

namespace NICK2._0_core.Controllers;

[ApiController]
[Route("api/dev/database")]
public sealed class DevDatabaseController(
    AppDbContext dbContext,
    IWebHostEnvironment environment,
    ILogger<DevDatabaseController> logger) : ControllerBase
{
    [HttpDelete("clear-data")]
    public async Task<IActionResult> ClearData(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var deletedProcessingStates = await dbContext.ProcessingStates
            .ExecuteDeleteAsync(cancellationToken);
        var deletedOpportunityStatusSnapshots = await dbContext.OpportunityStatusSnapshots
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "Cleared database table data. ProcessingStates: {ProcessingStateCount}. OpportunityStatusSnapshots: {OpportunityStatusSnapshotCount}.",
            deletedProcessingStates,
            deletedOpportunityStatusSnapshots);

        return Ok(new
        {
            deletedProcessingStates,
            deletedOpportunityStatusSnapshots
        });
    }
}
