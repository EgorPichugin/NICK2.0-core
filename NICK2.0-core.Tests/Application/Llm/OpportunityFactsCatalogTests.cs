using NICK2._0_core.Application.Llm;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Tests.Application.Llm;

public sealed class OpportunityFactsCatalogTests
{
    [Theory]
    [InlineData(EType.Unknown)]
    [InlineData(EType.Moving)]
    [InlineData(EType.Packing)]
    [InlineData(EType.MovingAndPacking)]
    [InlineData(EType.LoadOnly)]
    [InlineData(EType.UnloadOnly)]
    [InlineData(EType.Commercial)]
    [InlineData(EType.StorageInBound)]
    [InlineData(EType.StorageOutBound)]
    [InlineData(EType.InnerHouse)]
    [InlineData(EType.JunkRemoval)]
    [InlineData(EType.LaborOnly)]
    public void GetFacts_ReturnsFacts(EType type)
    {
        var result = OpportunityFactsCatalog.GetFacts(type);

        Assert.NotEmpty(result);
        Assert.All(result, fact =>
        {
            Assert.False(string.IsNullOrWhiteSpace(fact.Name));
            Assert.False(string.IsNullOrWhiteSpace(fact.Description));
        });
    }

    [Fact]
    public void GetFacts_ReturnsUnknownFacts_WhenTypeIsNotDefined()
    {
        var unknownFacts = OpportunityFactsCatalog.GetFacts(EType.Unknown);
        var undefinedFacts = OpportunityFactsCatalog.GetFacts((EType)999);

        Assert.Equal(unknownFacts, undefinedFacts);
    }
}
