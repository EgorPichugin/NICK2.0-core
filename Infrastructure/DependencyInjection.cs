using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Net.Http.Headers;
using System.Text;
using NICK2._0_core.Application.Aircall;
using NICK2._0_core.Application.SmartMoving;
using NICK2._0_core.Application.Webhooks.Repositories;
using NICK2._0_core.Infrastructure.Aircall;
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
        services.Configure<AircallOptions>(
            configuration.GetSection(AircallOptions.SectionName));
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

        services.AddHttpClient<IAircallClient, AircallClient>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<AircallOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                httpClient.BaseAddress = new Uri($"{settings.BaseUrl.TrimEnd('/')}/");
            }

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Content-Type",
                "application/json");

            if (!string.IsNullOrWhiteSpace(settings.ApiId)
                && !string.IsNullOrWhiteSpace(settings.ApiToken))
            {
                var credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{settings.ApiId}:{settings.ApiToken}"));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    credentials);
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
