using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Services;

namespace NICK2._0_core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProcessingStateService, ProcessingStateService>();
        services.AddScoped<IStatusValidationService, StatusValidationService>();
        services.AddScoped<IOpportunityStatusSnapshotService, OpportunityStatusSnapshotService>();
        services.AddScoped<IDataCollectorService, DataCollectorService>();
        services.AddScoped<IWebHookProcessingService, WebHookProcessingService>();

        return services;
    }
}
