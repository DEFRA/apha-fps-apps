using Amazon.EventBridge;
using Apha.BatchJobs.Pact.Api.Options;
using Apha.BatchJobs.Pact.Api.Services;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Diagnostics;

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

// Register BatchJobs infrastructure and services
var batchJobsConnectionString = builder.Configuration.GetConnectionString("BatchJobsConnectionString");
if (string.IsNullOrWhiteSpace(batchJobsConnectionString) || batchJobsConnectionString == "__REPLACE_VIA_ENV__")
{
    throw new InvalidOperationException(
        "ConnectionStrings:BatchJobsConnectionString is required. NoDb mode has been removed for PACT API.");
}

var dbCommandTimeoutSeconds = builder.Configuration.GetValue<int?>("BatchJobs:DbCommandTimeoutSeconds") is int v && v > 0 ? v : 30;

builder.Services.AddDbContext<BatchJobsDbContext>(
    options =>
    {
        options.UseNpgsql(
            batchJobsConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(dbCommandTimeoutSeconds);
            });
    },
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<BatchJobsDbContext>(options =>
{
    options.UseNpgsql(
        batchJobsConnectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(dbCommandTimeoutSeconds);
        });
});

builder.Services.AddScoped<IBatchLockRepository, BatchLockRepository>();
builder.Services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

builder.Services.Configure<EventPublisherOptions>(builder.Configuration.GetSection("EventBridge"));
builder.Services.Configure<TriggerDispatchOptions>(builder.Configuration.GetSection("TriggerDispatch"));
builder.Services.Configure<TriggerStoreOptions>(builder.Configuration.GetSection("TriggerStore"));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITriggerAttemptStore, MemoryTriggerAttemptStore>();
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

// Status query endpoints
app.MapGet("/api/batch-jobs/{jobName}/can-run", async (
    string jobName,
    IBatchLockRepository lockRepository,
    IJobExecutionRepository executionRepository,
    CancellationToken cancellationToken) =>
{
    try
    {
        var activeLock = await lockRepository.GetActiveLockAsync(jobName, cancellationToken);
        var lastExecution = await executionRepository.GetLastExecutionAsync(jobName, cancellationToken);

        var hasActiveExecution = lastExecution is not null
            && (lastExecution.Status == JobStatus.Running || lastExecution.Status == JobStatus.Pending || lastExecution.Status == JobStatus.Retry)
            && (lastExecution.CompletedAt is null);

        var canRun = activeLock is null && !hasActiveExecution;
        var reason = canRun
            ? null
            : activeLock is not null
                ? "Job is already running (active distributed lock)."
                : "Job has an active execution.";

        var result = new
        {
            jobName,
            canRun,
            reason,
            activeLock = activeLock is null ? null : new
            {
                activeLock.JobQueueId,
                activeLock.AcquiredAt,
                activeLock.ExpiresAt,
                activeLock.IsActive
            },
            sourceOfTruth = "BatchJobs"
        };
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to check can-run status: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/batch-jobs/{jobName}/status", async (
    string jobName,
    [FromQuery] string? jobExecutionId,
    ITriggerAttemptStore triggerAttemptStore,
    IJobExecutionRepository executionRepository,
    IHostEnvironment environment,
    CancellationToken cancellationToken) =>
{
    try
    {
        TriggerAttemptRecord? triggerAttempt = null;

        if (!string.IsNullOrWhiteSpace(jobExecutionId))
        {
            triggerAttempt = await triggerAttemptStore.GetByJobExecutionIdAsync(jobExecutionId, cancellationToken);
        }

        var execution = Guid.TryParse(jobExecutionId, out var correlatedExecutionId)
            ? await executionRepository.GetExecutionByJobExecutionIdAsync(correlatedExecutionId, cancellationToken)
            : await executionRepository.GetLastExecutionAsync(jobName, cancellationToken);

        if (triggerAttempt is null)
        {
            triggerAttempt = await triggerAttemptStore.GetLatestByJobNameAsync(jobName, cancellationToken);
        }

        object? startupWatchdog = null;
        var isRunning = false;
        var sourceOfTruth = "BatchJobs";
        var correlatedJobExecutionId = jobExecutionId;

        if (execution is not null)
        {
            correlatedJobExecutionId = execution.JobExecutionId.ToString("D");
        }
        else if (triggerAttempt is not null)
        {
            correlatedJobExecutionId = triggerAttempt.JobExecutionId;
        }

        if (execution is null && triggerAttempt is not null)
        {
            var now = DateTime.UtcNow;
            var startupSlaSeconds = environment.IsProduction() ? 600 : 30;
            var startupDeadlineUtc = triggerAttempt.AcceptedAtUtc.AddSeconds(startupSlaSeconds);
            var workerProcessExited = false;

            if (triggerAttempt.EventId.StartsWith("localproc-", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(triggerAttempt.EventId["localproc-".Length..], out var workerPid))
            {
                try
                {
                    var process = Process.GetProcessById(workerPid);
                    workerProcessExited = process.HasExited;
                }
                catch
                {
                    workerProcessExited = true;
                }
            }

            var projectedState = workerProcessExited
                ? "WorkerProcessExited"
                : now > startupDeadlineUtc
                    ? "StartFailedTimeout"
                    : triggerAttempt.WorkerProcessLaunched
                        ? "WorkerProcessStarted"
                        : "TriggerAccepted";

            startupWatchdog = new
            {
                projectedState,
                acceptedAtUtc = triggerAttempt.AcceptedAtUtc,
                startupDeadlineUtc,
                evaluatedAtUtc = now,
                startupSlaSeconds,
                deliveryExhaustionConfirmed = false,
                deliveryExhaustionOwner = "IntegrationTransportReconciler",
                eventId = triggerAttempt.EventId,
                triggerStore = "PactInMemoryCache"
            };

            isRunning = projectedState != "StartFailedTimeout" && projectedState != "WorkerProcessExited";
            sourceOfTruth = "StartupWatchdog";
        }
        else if (execution is not null)
        {
            isRunning = execution.Status == JobStatus.Running || execution.Status == JobStatus.Pending || execution.Status == JobStatus.Retry;
        }

        var result = new
        {
            jobName,
            isRunning,
            sourceOfTruth,
            correlatedJobExecutionId,
            lastExecution = execution is null ? null : new
            {
                execution.ExecutionId,
                execution.JobName,
                execution.JobExecutionId,
                status = execution.Status.ToString(),
                startedAt = execution.StartedAt,
                completedAt = execution.CompletedAt,
                durationSeconds = execution.DurationSeconds,
                recordsProcessed = execution.RecordsProcessed,
                recordsFailed = execution.RecordsFailed,
                errorMessage = execution.ErrorMessage
            },
            startupWatchdog
        };

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch status: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
