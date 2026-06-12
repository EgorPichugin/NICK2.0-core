using System.Net.Http.Json;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Infrastructure.SmartMoving.Mappers;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

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

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new SmartMovingApiException(response.StatusCode, responseBody);
        }

        var opportunityResponse = await response.Content.ReadFromJsonAsync<SmartMovingOpportunityApiResponse>(
            cancellationToken);

        return opportunityResponse?.ToSmartMovingOpportunity();
    }
}
