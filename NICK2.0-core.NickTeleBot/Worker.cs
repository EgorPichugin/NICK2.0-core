using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Application.Llm;
using NICK2._0_core.Application.Processing;
using NICK2._0_core.Domain;
using NICK2._0_core.Infrastructure.Persistence;
using Telegram.Bot;

namespace NICK2._0_core.NickTeleBot;

public class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const long ChatId = 859869583;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = configuration["TelegramBot:Token"]
            ?? configuration["TELEGRAM_BOT_TOKEN"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Telegram bot token is missing. Set TELEGRAM_BOT_TOKEN.");

        var botClient = new TelegramBotClient(token);
        var me = await botClient.GetMe(stoppingToken);

        logger.LogInformation("Telegram bot @{Username} started", me.Username);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendNextReadyMessage(botClient, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to send Telegram outbox message");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task SendNextReadyMessage(
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var message = await dbContext.TelegramOutboxMessages
            .Where(message => message.Status == ETelegramOutboxMessageStatus.Ready)
            .OrderBy(message => message.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (message is null)
            return;

        await botClient.SendMessage(
            chatId: ChatId,
            text: FormatFactsMessage(message.PayloadJson),
            cancellationToken: cancellationToken);

        message.Status = ETelegramOutboxMessageStatus.Sent;
        message.SentAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Sent Telegram outbox message {MessageId}", message.Id);
    }

    private static string FormatFactsMessage(string payloadJson)
    {
        var processingData = JsonSerializer.Deserialize<OpportunityProcessingData>(
            payloadJson,
            SerializerOptions);
        var facts = processingData?.AnalysisResult?.Summary.Facts;

        if (facts is null || facts.Count == 0)
            return "No analyzed facts found.";

        var builder = new StringBuilder("Analyzed facts:");

        foreach (var fact in facts)
        {
            AppendFact(builder, fact);
        }

        return builder.ToString();
    }

    private static void AppendFact(StringBuilder builder, LlmOpportunityAnalyzedFact fact)
    {
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine(fact.Name);
        builder.AppendLine($"Status: {fact.Status}");
        builder.AppendLine($"Confidence: {fact.Confidence}");
        builder.AppendLine($"Requires human review: {fact.RequiresHumanReview}");
        builder.AppendLine($"Call value: {fact.CallValue ?? "-"}");
        builder.AppendLine($"Crew notes value: {fact.CrewNotesValue ?? "-"}");
        builder.AppendLine($"Explanation: {fact.Explanation}");
    }
}
