using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace NICK2._0_core.Infrastructure.Llm;

public sealed class OpenAiChatCompletionClient(IOptions<LlmOptions> options) : ILlmCompletionClient
{
    private const string SystemPrompt = """
        You are an operations QA assistant for a moving company.
        Use only the data provided in the user message.
        Do not invent missing facts or values.
        Keep evidence quotes short and tied to the provided source.
        Return only valid JSON that matches the requested schema.
        Use low confidence and require human review when evidence is missing, ambiguous, or conflicting.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly Lazy<ChatClient> chatClient = new(
        () => CreateChatClient(options.Value),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<string> CreateStructuredResponse(
        string prompt,
        string schemaName,
        object schema,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        ValidateSettings(settings);

        ChatMessage[] messages =
        [
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(prompt)
        ];
        var completionOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: schemaName,
                jsonSchema: BinaryData.FromString(JsonSerializer.Serialize(schema, JsonOptions)),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await chatClient.Value.CompleteChatAsync(
            messages,
            completionOptions,
            cancellationToken);

        if (completion.Content.Count == 0
            || string.IsNullOrWhiteSpace(completion.Content[0].Text))
        {
            throw new InvalidOperationException("LLM response output text was empty.");
        }

        return completion.Content[0].Text;
    }

    private static ChatClient CreateChatClient(LlmOptions settings)
    {
        ValidateSettings(settings);

        var model = ResolveModel(settings);
        var apiKey = ResolveApiKey(settings);
        var clientOptions = new OpenAIClientOptions();
        var baseUrl = ResolveBaseUrl(settings);
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            clientOptions.Endpoint = new Uri(baseUrl);
        }

        return new ChatClient(
            model,
            new ApiKeyCredential(apiKey),
            clientOptions);
    }

    private static void ValidateSettings(LlmOptions settings)
    {
        if (string.IsNullOrWhiteSpace(ResolveApiKey(settings)))
        {
            throw new InvalidOperationException("LLM API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(ResolveModel(settings)))
        {
            throw new InvalidOperationException("LLM model is not configured.");
        }
    }

    private static string ResolveApiKey(LlmOptions settings)
    {
        return FirstConfigured(
            settings.ApiKey,
            Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    }

    private static string ResolveModel(LlmOptions settings)
    {
        return FirstConfigured(
            settings.Model,
            Environment.GetEnvironmentVariable("OPENAI_MODEL"));
    }

    private static string ResolveBaseUrl(LlmOptions settings)
    {
        return FirstConfigured(
            settings.BaseUrl,
            Environment.GetEnvironmentVariable("OPENAI_BASE_URL"));
    }

    private static string FirstConfigured(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
