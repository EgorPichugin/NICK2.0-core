namespace NICK2._0_core.Domain;

public sealed class OpportunityStatusSnapshot
{
    public Guid Id { get; set; }

    public OpportunityStatus CurrentStatus { get; set; }

    public OpportunityStatus? PreviousStatus { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
