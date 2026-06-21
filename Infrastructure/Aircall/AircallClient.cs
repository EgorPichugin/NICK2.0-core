using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Options;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.Developer;
using NICK2._0_core.Infrastructure.Aircall.Mappers;
using NICK2._0_core.Infrastructure.Aircall.Responses;

namespace NICK2._0_core.Infrastructure.Aircall;

/// <inheritdoc />
public sealed class AircallClient(
    HttpClient httpClient,
    IOptions<AircallOptions> options,
    ILogger<AircallClient> logger) : IAircallClient
{
    /// <inheritdoc />
    public async Task<DevAircallCallSearchResponse> SearchCallsForDeveloper(
        string customerPhoneNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerPhoneNumber))
        {
            return new DevAircallCallSearchResponse([]);
        }

        return new DevAircallCallSearchResponse(
            await GetRawCallSearchPages(customerPhoneNumber, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<DevAircallCallAiSummaryResponse?> GetCallAiSummaryForDeveloper(
        long callId,
        CancellationToken cancellationToken)
    {
        var summary = await GetRawCallSummary(callId, cancellationToken);

        return summary is null
            ? null
            : new DevAircallCallAiSummaryResponse(summary.Value);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AircallCallSearchResult>> SearchCalls(
        string customerPhoneNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerPhoneNumber))
        {
            return [];
        }

        var callSummaries = await GetCallSummaries(customerPhoneNumber, cancellationToken);

        return callSummaries
            .Select(callSummary => callSummary.ToAircallCallSearchResult())
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<AircallCallAiSummary?> GetCallAiSummary(
        long callId,
        CancellationToken cancellationToken)
    {
        var summary = await GetCallSummary(callId, cancellationToken);

        return summary?.ToAircallCallAiSummary();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AircallCall>> GetCalls(
        string customerPhoneNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerPhoneNumber))
        {
            return [];
        }

        var callSummaries = await GetCallSummaries(customerPhoneNumber, cancellationToken);
        var calls = new List<AircallCall>();
        var callOrder = 1;

        foreach (var callSummary in callSummaries)
        {
            var summary = await GetCallSummary(callSummary.Id, cancellationToken);

            if (summary is not null)
            {
                calls.Add(callSummary.ToAircallCall(summary, callOrder));
            }

            callOrder++;
        }

        logger.LogDebug(
            "Received {CallCount} Aircall calls for customer phone number {CustomerPhoneNumber}.",
            calls.Count,
            customerPhoneNumber);

        return calls;
    }

    private async Task<IReadOnlyCollection<AircallCallSummaryApiResponse>> GetCallSummaries(
        string customerPhoneNumber,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var calls = new List<AircallCallSummaryApiResponse>();
        var requestUri = BuildCallsRequestUri(settings, customerPhoneNumber);

        while (!string.IsNullOrWhiteSpace(requestUri))
        {
            logger.LogDebug(
                "Sending Aircall calls request for customer phone number {CustomerPhoneNumber}.",
                customerPhoneNumber);

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogWarning(
                    "Aircall calls request failed for customer phone number {CustomerPhoneNumber}. StatusCode: {StatusCode}.",
                    customerPhoneNumber,
                    response.StatusCode);

                throw new AircallApiException(response.StatusCode, responseBody);
            }

            var callsResponse = await response.Content.ReadFromJsonAsync<AircallCallsApiResponse>(
                cancellationToken);

            if (callsResponse is null)
            {
                logger.LogWarning(
                    "Aircall calls response for customer phone number {CustomerPhoneNumber} was empty.",
                    customerPhoneNumber);

                break;
            }

            calls.AddRange(callsResponse.Calls);
            requestUri = callsResponse.Meta?.NextPageLink;
        }

        return calls;
    }

    private async Task<IReadOnlyCollection<JsonElement>> GetRawCallSearchPages(
        string customerPhoneNumber,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var pages = new List<JsonElement>();
        var requestUri = BuildCallsRequestUri(settings, customerPhoneNumber);

        while (!string.IsNullOrWhiteSpace(requestUri))
        {
            logger.LogDebug(
                "Sending raw Aircall calls request for customer phone number {CustomerPhoneNumber}.",
                customerPhoneNumber);

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogWarning(
                    "Raw Aircall calls request failed for customer phone number {CustomerPhoneNumber}. StatusCode: {StatusCode}.",
                    customerPhoneNumber,
                    response.StatusCode);

                throw new AircallApiException(response.StatusCode, responseBody);
            }

            var page = await ReadRawJsonResponse(response, cancellationToken);

            if (page is null)
            {
                logger.LogWarning(
                    "Raw Aircall calls response for customer phone number {CustomerPhoneNumber} was empty.",
                    customerPhoneNumber);

                break;
            }

            pages.Add(page.Value);
            requestUri = TryGetNextPageLink(page.Value);
        }

        return pages;
    }

    private async Task<AircallCallAiSummaryApiResponse?> GetCallSummary(
        long callId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending Aircall summary request for call {CallId}.",
            callId);

        var path = string.Format(
            options.Value.CallSummaryPathTemplate,
            callId);

        using var response = await httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "Aircall summary for call {CallId} was not found. StatusCode: {StatusCode}.",
                callId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "Aircall summary request failed for call {CallId}. StatusCode: {StatusCode}.",
                callId,
                response.StatusCode);

            throw new AircallApiException(response.StatusCode, responseBody);
        }

        return await response.Content.ReadFromJsonAsync<AircallCallAiSummaryApiResponse>(
            cancellationToken);
    }

    private async Task<JsonElement?> GetRawCallSummary(
        long callId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Sending raw Aircall summary request for call {CallId}.",
            callId);

        var path = string.Format(
            options.Value.CallSummaryPathTemplate,
            callId);

        using var response = await httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation(
                "Raw Aircall summary for call {CallId} was not found. StatusCode: {StatusCode}.",
                callId,
                response.StatusCode);

            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogWarning(
                "Raw Aircall summary request failed for call {CallId}. StatusCode: {StatusCode}.",
                callId,
                response.StatusCode);

            throw new AircallApiException(response.StatusCode, responseBody);
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

    private static string? TryGetNextPageLink(JsonElement page)
    {
        if (!page.TryGetProperty("meta", out var meta)
            || !meta.TryGetProperty("next_page_link", out var nextPageLink)
            || nextPageLink.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return nextPageLink.GetString();
    }

    private static string BuildCallsRequestUri(
        AircallOptions settings,
        string customerPhoneNumber)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query[settings.PhoneNumberQueryParameter] = NormalizePhoneNumber(customerPhoneNumber);

        return $"{settings.CallsPath.TrimStart('/')}?{query}";
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = Regex.Replace(phoneNumber, "\\D", string.Empty);

        return digits.Length switch
        {
            10 => $"+1{digits}",
            11 when digits.StartsWith('1') => $"+{digits}",
            _ when phoneNumber.TrimStart().StartsWith('+') => $"+{digits}",
            _ => phoneNumber
        };
    }
}
