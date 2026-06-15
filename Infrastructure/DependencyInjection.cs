using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Infrastructure.Persistence;
using NICK2._0_core.Infrastructure.Persistence.Repositories;
using NICK2._0_core.Infrastructure.SmartMoving;

namespace NICK2._0_core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmartMovingOptions>(
            configuration.GetSection(SmartMovingOptions.SectionName));
        services.Configure<PostgreSqlOptions>(
            configuration.GetSection(PostgreSqlOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(BuildPostgreSqlConnectionString(configuration)));
        services.AddHostedService<DatabaseInitializer>();
        services.AddScoped<IOpportunityStatusSnapshotRepository, OpportunityStatusSnapshotRepository>();
        services.AddScoped<IProcessingStateRepository, ProcessingStateRepository>();

        services.AddHttpClient<ISmartMovingClient, SmartMovingClient>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SmartMovingOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                httpClient.BaseAddress = new Uri($"{settings.BaseUrl.TrimEnd('/')}/");
            }

            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
            }
        });

        return services;
    }

    private static string BuildPostgreSqlConnectionString(IConfiguration configuration)
    {
        var options = configuration
            .GetSection(PostgreSqlOptions.SectionName)
            .Get<PostgreSqlOptions>() ?? new PostgreSqlOptions();

        return new NpgsqlConnectionStringBuilder
        {
            Host = options.Host,
            Port = options.Port,
            Database = options.Database,
            Username = options.Username,
            Password = options.Password
        }.ConnectionString;
    }
}
