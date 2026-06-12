using Microsoft.Extensions.Options;
using NICK2._0_core.Application.Webhooks;
using NICK2._0_core.Application.Webhooks.Services;
using NICK2._0_core.Infrastructure.SmartMoving;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SmartMovingOptions>(
    builder.Configuration.GetSection(SmartMovingOptions.SectionName));
builder.Services.AddHttpClient<ISmartMovingClient, SmartMovingClient>((serviceProvider, httpClient) =>
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
builder.Services.AddSingleton<IProcessingStateService, ProcessingStateService>();
builder.Services.AddScoped<IStatusValidationService, StatusValidationService>();
builder.Services.AddScoped<IDataCollectorService, DataCollectorService>();
builder.Services.AddScoped<IWebHookProcessingService, WebHookProcessingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();
