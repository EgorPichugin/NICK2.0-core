using System.Text.Json;
using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Developer;

public sealed record DevAircallCallAiSummaryResponse(
    [property: JsonPropertyName("response")]
    JsonElement Response);
