using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Domain;
using NICK2._0_core.Infrastructure.Llm;

namespace NICK2._0_core.Tests.Infrastructure.Llm;

public sealed class LlmServiceTests
{
    [Fact]
    public async Task AnalyzeOpportunity_ReturnsMockedResult()
    {
        var opportunity = new SmartMovingOpportunity(
            Guid.NewGuid(),
            90690,
            "7472874522",
            EType.Moving,
            3,
            DateTimeOffset.Parse("2026-06-18T00:00:00Z"),
            null,
            null,
            [],
            []);
        var service = new LlmService();

        var result = await service.AnalyzeOpportunity(opportunity, CancellationToken.None);

        Assert.Equal(opportunity.Id, result.OpportunityId);
        Assert.True(result.IsMocked);
        Assert.Contains(opportunity.Id.ToString(), result.Summary);
    }
}
