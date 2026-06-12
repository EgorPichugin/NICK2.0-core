using NICK2._0_core.Application.Webhooks.Services;

namespace NICK2._0_core.Tests.Application.Webhooks.Services;

public sealed class StatusValidationServiceTests
{
    private readonly StatusValidationService _service = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(50)]
    public async Task IsValid_ReturnsTrue_WhenStatusIsKnown(int status)
    {
        var result = await _service.IsValid(status, CancellationToken.None);

        Assert.True(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(999)]
    public async Task IsValid_ReturnsFalse_WhenStatusIsUnknown(int status)
    {
        var result = await _service.IsValid(status, CancellationToken.None);

        Assert.False(result);
    }
}
