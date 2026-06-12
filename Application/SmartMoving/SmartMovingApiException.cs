using System.Net;

namespace NICK2._0_core.Application.SmartMoving;

public sealed class SmartMovingApiException : Exception
{
    public SmartMovingApiException(
        HttpStatusCode statusCode,
        string responseBody)
        : base($"SmartMoving API request failed with status code {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }
}
