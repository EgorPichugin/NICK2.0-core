using System.Net;

namespace NICK2._0_core.Application.Aircall;

public sealed class AircallApiException : Exception
{
    public AircallApiException(
        HttpStatusCode statusCode,
        string responseBody)
        : base($"Aircall API request failed with status code {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }
}
