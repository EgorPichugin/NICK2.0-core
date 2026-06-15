using System.Net.Http.Json;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Infrastructure.SmartMoving.Mappers;
using NICK2._0_core.Infrastructure.SmartMoving.Responses;

namespace NICK2._0_core.Infrastructure.SmartMoving;

/// <inheritdoc />
public sealed class SmartMovingClient(
    HttpClient httpClient,
    ILogger<SmartMovingClient> logger) : ISmartMovingClient
{
    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> GetOpportunity(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending SmartMoving opportunity request for {OpportunityId}.",
            opportunityId);

        using var response = await httpClient.GetAsync(
            $"opportunities/{opportunityId}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "SmartMoving opportunity {OpportunityId} was not found. StatusCode: {StatusCode}.",
                opportunityId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "SmartMoving opportunity request failed for {OpportunityId}. StatusCode: {StatusCode}.",
                opportunityId,
                response.StatusCode);

            throw new SmartMovingApiException(response.StatusCode, responseBody);
        }

        var opportunityResponse = await response.Content.ReadFromJsonAsync<SmartMovingOpportunityApiResponse>(
            cancellationToken);

        var opportunity = opportunityResponse?.ToSmartMovingOpportunity();

        if (opportunity is null)
        {
            logger.LogWarning(
                "SmartMoving opportunity response for {OpportunityId} was empty or could not be mapped.",
                opportunityId);
        }
        else
        {
            logger.LogDebug(
                "Received SmartMoving opportunity {OpportunityId}.",
                opportunityId);
        }

        return opportunity;
    }
}
