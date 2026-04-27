using Apha.BatchJobs.Api.Services;
using Apha.BatchJobs.Worker;
using Amazon.ECS;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Reuse the exact same service registration as the batch worker.
// This wires up domain, infrastructure and application layers (repos, factories, etc.)
ServiceCollectionSetup.ConfigureBatchJobServices(builder.Services, builder.Configuration);

builder.Services.AddScoped<IJobStatusService, JobStatusService>();
builder.Services.AddAWSService<IAmazonECS>();
builder.Services.AddScoped<IEcsTaskDispatcher, EcsTaskDispatcher>();
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
