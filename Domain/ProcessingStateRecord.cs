namespace NICK2._0_core.Domain;

public sealed class ProcessingStateRecord
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid OpportunityId { get; set; }

    public EOpportunityStatus OpportunityStatus { get; set; }

    public EProcessingState State { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}
