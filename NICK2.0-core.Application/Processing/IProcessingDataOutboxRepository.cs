namespace NICK2._0_core.Application.Processing;

public interface IProcessingDataOutboxRepository
{
    Task AddProcessingData(
        OpportunityProcessingData processingData,
        CancellationToken cancellationToken);
}
