namespace NICK2._0_core.Infrastructure.SmartMoving;

/// <inheritdoc />
public sealed class SmartMovingClient(
    HttpClient httpClient) : ISmartMovingClient
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> GetOpportunity(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"opportunities/{opportunityId}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return new SmartMovingOpportunity(opportunityId);
    }
}
