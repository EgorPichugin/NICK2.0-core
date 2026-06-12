using Microsoft.Extensions.Options;
using NICK2._0_core.Application.SmartMoving;
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
}
