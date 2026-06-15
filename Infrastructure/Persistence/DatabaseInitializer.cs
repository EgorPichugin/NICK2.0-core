using Microsoft.EntityFrameworkCore;

namespace NICK2._0_core.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IServiceProvider serviceProvider,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.LogInformation("Applying database migrations.");

        await dbContext.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
