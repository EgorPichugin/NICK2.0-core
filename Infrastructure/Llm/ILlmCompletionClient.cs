namespace NICK2._0_core.Infrastructure.Llm;

public interface ILlmCompletionClient
{
    Task<string> CreateStructuredResponse(
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken);
}
