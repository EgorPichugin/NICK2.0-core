using System.Text.Json.Serialization;

namespace NICK2._0_core.Application.Developer;

public sealed record DevAircallCallAiSummaryBatchRequest(
    [property: JsonPropertyName("callIds")]
    IReadOnlyCollection<long> CallIds);
