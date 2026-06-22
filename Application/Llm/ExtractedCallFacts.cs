namespace NICK2._0_core.Application.Llm;

public sealed record ExtractedCallFacts(
    IReadOnlyCollection<ExtractedCallFact> Facts,
    string Confidence,
    bool RequiresHumanReview);

public sealed record ExtractedCallFact(
    string Name,
    string Description,
    string? Value,
    string Status,
    string Confidence,
    bool RequiresHumanReview,
    IReadOnlyCollection<ExtractedCallFactEvidence> Evidence,
    string? Notes);

public sealed record ExtractedCallFactEvidence(
    string CallId,
    string Quote);
