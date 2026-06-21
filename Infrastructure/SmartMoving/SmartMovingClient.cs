using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using NICK2._0_core.Application.Developer;
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
    public async Task<DevSmartMovingOpportunityResponse?> GetOpportunityForDeveloper(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        var rawOpportunity = await GetRawOpportunityResponse(opportunityId, cancellationToken);

        if (rawOpportunity is null)
        {
            return null;
        }

        var jobs = await GetRawJobs(
            opportunityId,
            rawOpportunity.Value,
            cancellationToken);

        return new DevSmartMovingOpportunityResponse(
            rawOpportunity.Value,
            jobs);
    }

    /// <inheritdoc />
    public async Task<SmartMovingOpportunity?> GetOpportunity(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        var opportunityResponse = await GetOpportunityResponse(opportunityId, cancellationToken);

        if (opportunityResponse is null)
        {
            return null;
        }

        var jobs = await GetJobs(
            opportunityResponse,
            cancellationToken);

        var opportunity = opportunityResponse.ToSmartMovingOpportunity(jobs);

        logger.LogDebug(
            "Received SmartMoving opportunity {OpportunityId} with {JobCount} jobs.",
            opportunityId,
            opportunity.Jobs.Count);

        return opportunity;
    }

    private async Task<SmartMovingOpportunityApiResponse?> GetOpportunityResponse(
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

        if (opportunityResponse is null)
        {
            logger.LogWarning(
                "SmartMoving opportunity response for {OpportunityId} was empty.",
                opportunityId);
        }

        return opportunityResponse;
    }

    private async Task<JsonElement?> GetRawOpportunityResponse(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending raw SmartMoving opportunity request for {OpportunityId}.",
            opportunityId);

        using var response = await httpClient.GetAsync(
            $"opportunities/{opportunityId}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "Raw SmartMoving opportunity {OpportunityId} was not found. StatusCode: {StatusCode}.",
                opportunityId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "Raw SmartMoving opportunity request failed for {OpportunityId}. StatusCode: {StatusCode}.",
                opportunityId,
                response.StatusCode);

            throw new SmartMovingApiException(response.StatusCode, responseBody);
        }

        return await ReadRawJsonResponse(response, cancellationToken);
    }

    private async Task<IReadOnlyCollection<SmartMovingJobDetailsApiResponse>> GetJobs(
        SmartMovingOpportunityApiResponse opportunityResponse,
        CancellationToken cancellationToken)
    {
        var jobs = new List<SmartMovingJobDetailsApiResponse>();

        foreach (var job in opportunityResponse.Jobs)
        {
            var jobDetails = await GetJob(
                opportunityResponse.Id,
                job.Id,
                cancellationToken);

            if (jobDetails is not null)
            {
                jobs.Add(jobDetails);
            }
        }

        return jobs;
    }

    private async Task<IReadOnlyCollection<JsonElement>> GetRawJobs(
        Guid opportunityId,
        JsonElement rawOpportunity,
        CancellationToken cancellationToken)
    {
        var jobs = new List<JsonElement>();

        foreach (var jobId in GetRawOpportunityJobIds(rawOpportunity))
        {
            var jobDetails = await GetRawJob(
                opportunityId,
                jobId,
                cancellationToken);

            if (jobDetails is not null)
            {
                jobs.Add(jobDetails.Value);
            }
        }

        return jobs;
    }

    private static IReadOnlyCollection<Guid> GetRawOpportunityJobIds(JsonElement rawOpportunity)
    {
        if (!TryGetPropertyIgnoreCase(rawOpportunity, "jobs", out var jobs)
            || jobs.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var jobIds = new List<Guid>();

        foreach (var job in jobs.EnumerateArray())
        {
            if (!TryGetPropertyIgnoreCase(job, "id", out var id)
                || id.ValueKind != JsonValueKind.String
                || !Guid.TryParse(id.GetString(), out var jobId))
            {
                continue;
            }

            jobIds.Add(jobId);
        }

        return jobIds;
    }

    private static bool TryGetPropertyIgnoreCase(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;

                return true;
            }
        }

        value = default;

        return false;
    }

    private async Task<SmartMovingJobDetailsApiResponse?> GetJob(
        Guid opportunityId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending SmartMoving job request for opportunity {OpportunityId}, job {JobId}.",
            opportunityId,
            jobId);

        var path = $"premium/opportunities/{opportunityId}/jobs/{jobId}";
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["IncludeNotes"] = "true";

        using var response = await httpClient.GetAsync(
            $"{path}?{query}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "SmartMoving job {JobId} for opportunity {OpportunityId} was not found. StatusCode: {StatusCode}.",
                jobId,
                opportunityId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "SmartMoving job request failed for opportunity {OpportunityId}, job {JobId}. StatusCode: {StatusCode}.",
                opportunityId,
                jobId,
                response.StatusCode);

            throw new SmartMovingApiException(response.StatusCode, responseBody);
        }

        return await response.Content.ReadFromJsonAsync<SmartMovingJobDetailsApiResponse>(
            cancellationToken);
    }

    private async Task<JsonElement?> GetRawJob(
        Guid opportunityId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending raw SmartMoving job request for opportunity {OpportunityId}, job {JobId}.",
            opportunityId,
            jobId);

        var path = $"premium/opportunities/{opportunityId}/jobs/{jobId}";
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["IncludeNotes"] = "true";

        using var response = await httpClient.GetAsync(
            $"{path}?{query}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "Raw SmartMoving job {JobId} for opportunity {OpportunityId} was not found. StatusCode: {StatusCode}.",
                jobId,
                opportunityId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "Raw SmartMoving job request failed for opportunity {OpportunityId}, job {JobId}. StatusCode: {StatusCode}.",
                opportunityId,
                jobId,
                response.StatusCode);

            throw new SmartMovingApiException(response.StatusCode, responseBody);
        }

        return await ReadRawJsonResponse(response, cancellationToken);
    }

    private static async Task<JsonElement?> ReadRawJsonResponse(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.Clone();
    }
}
