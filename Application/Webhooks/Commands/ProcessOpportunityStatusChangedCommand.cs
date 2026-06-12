using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Webhooks.Commands;

public sealed record ProcessOpportunityStatusChangedCommand(
    [property: JsonPropertyName("event-type")] string EventType,
    [property: JsonPropertyName("opportunity-id")] Guid OpportunityId,
    [property: JsonPropertyName("opportunity-status")] int OpportunityStatus)
{
    public const string ExpectedEventType = "opportunity-status-changed";
}
