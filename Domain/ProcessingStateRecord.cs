namespace NICK2._0_core.Domain;

public sealed class ProcessingStateRecord
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid OpportunityId { get; set; }

    public OpportunityStatus OpportunityStatus { get; set; }

    public ProcessingState State { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}
