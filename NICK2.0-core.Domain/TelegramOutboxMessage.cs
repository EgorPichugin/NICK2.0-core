namespace NICK2._0_core.Domain;

public sealed class TelegramOutboxMessage
{
    public Guid Id { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public ETelegramOutboxMessageStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }
}
