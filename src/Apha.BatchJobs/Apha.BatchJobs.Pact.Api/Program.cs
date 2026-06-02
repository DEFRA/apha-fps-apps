using Amazon.EventBridge;
using Apha.BatchJobs.Pact.Api.Options;
using Apha.BatchJobs.Pact.Api.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BatchJobs PACT API",
        Version = "v1",
        Description = "Batch jobs trigger API for PACT routes"
    });
});
builder.Services.Configure<EventPublisherOptions>(builder.Configuration.GetSection("EventBridge"));
builder.Services.Configure<TriggerDispatchOptions>(builder.Configuration.GetSection("TriggerDispatch"));
builder.Services.AddAWSService<IAmazonEventBridge>();
builder.Services.AddScoped<IEventPublisher, EventBridgePublisher>();
builder.Services.AddScoped<EventBridgeTriggerDispatcher>();
builder.Services.AddScoped<LocalWorkerProcessTriggerDispatcher>();
builder.Services.AddScoped<ITriggerDispatcher>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TriggerDispatchOptions>>().Value;
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("TriggerDispatcherResolver");

    if (string.Equals(options.Mode, "LocalProcess", StringComparison.OrdinalIgnoreCase))
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
        {
            logger.LogInformation("Trigger dispatch mode set to LocalProcess for environment {EnvironmentName}", environment.EnvironmentName);
            return serviceProvider.GetRequiredService<LocalWorkerProcessTriggerDispatcher>();
        }

        logger.LogWarning(
            "Trigger dispatch mode LocalProcess is not allowed in environment {EnvironmentName}; falling back to EventBridge.",
            environment.EnvironmentName);
    }

    return serviceProvider.GetRequiredService<EventBridgeTriggerDispatcher>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BatchJobs PACT API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "pact.api", timestamp = DateTime.UtcNow }));

app.Run();
