using System.Text.Json;
using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Developer;

public sealed record DevAircallCallAiSummaryBatchResponse(
    [property: JsonPropertyName("summaries")]
    IReadOnlyCollection<JsonElement> Summaries,
    [property: JsonPropertyName("notFoundCallIds")]
    IReadOnlyCollection<long> NotFoundCallIds);
