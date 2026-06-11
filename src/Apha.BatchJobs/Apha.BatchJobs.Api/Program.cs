using Apha.BatchJobs.Api.Services;
using Apha.BatchJobs.Application.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Reuse the exact same service registration as the batch worker.
// This wires up domain, infrastructure and application layers (repos, factories, etc.)
ServiceCollectionSetup.ConfigureBatchJobServices(builder.Services, builder.Configuration);

builder.Services.Configure<StartupWatchdogOptions>(builder.Configuration.GetSection("StartupWatchdog"));
builder.Services.AddScoped<IJobStatusService, JobStatusService>();
builder.Services.AddControllers();

var app = builder.Build();

// Global exception handler - returns structured JSON error
app.UseExceptionHandler(errApp => errApp.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var ex = feature?.Error;
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new
    {
        error = "An unexpected error occurred.",
        detail = ex?.Message
    });
}));

app.MapControllers();

// Lightweight health probe for load balancer / ECS health checks
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
