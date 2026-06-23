using System.Text.Json;
using NICK2._0_core.Application.Processing;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence.Repositories;

public sealed class ProcessingDataOutboxRepository(AppDbContext dbContext)
    : IProcessingDataOutboxRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task AddProcessingData(
        OpportunityProcessingData processingData,
        CancellationToken cancellationToken)
    {
        var message = new TelegramOutboxMessage
        {
            Id = Guid.NewGuid(),
            PayloadJson = JsonSerializer.Serialize(processingData, SerializerOptions),
            Status = ETelegramOutboxMessageStatus.Ready,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.TelegramOutboxMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
