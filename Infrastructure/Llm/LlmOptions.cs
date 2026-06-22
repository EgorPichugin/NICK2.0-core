namespace NICK2._0_core.Infrastructure.Llm;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;
}
