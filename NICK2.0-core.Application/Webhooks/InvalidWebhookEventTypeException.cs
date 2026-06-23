namespace NICK2._0_core.Application.Webhooks;

public sealed class InvalidWebhookEventTypeException : Exception
{
    public InvalidWebhookEventTypeException(string actualEventType, string expectedEventType)
        : base($"Invalid webhook event type '{actualEventType}'. Expected '{expectedEventType}'.")
    {
        ActualEventType = actualEventType;
        ExpectedEventType = expectedEventType;
    }

    public string ActualEventType { get; }

    public string ExpectedEventType { get; }
}
