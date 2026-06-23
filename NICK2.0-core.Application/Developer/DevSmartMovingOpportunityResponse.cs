using System.Text.Json;
using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Developer;

public sealed record DevSmartMovingOpportunityResponse(
    [property: JsonPropertyName("opportunity")]
    JsonElement Opportunity,
    [property: JsonPropertyName("jobs")]
    IReadOnlyCollection<JsonElement> Jobs);
