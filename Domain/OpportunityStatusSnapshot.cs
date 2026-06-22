namespace NICK2._0_core.Domain;

public sealed class OpportunityStatusSnapshot
{
    public Guid Id { get; set; }

    public EOpportunityStatus CurrentStatus { get; set; }

    public EOpportunityStatus? PreviousStatus { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
