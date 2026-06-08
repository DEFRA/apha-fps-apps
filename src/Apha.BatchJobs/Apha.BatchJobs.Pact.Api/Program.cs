using Amazon.EventBridge;
using Asp.Versioning;
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
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
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

var pactEventPublisherOptions = builder.Configuration.GetSection("EventBridge").Get<EventPublisherOptions>()
    ?? new EventPublisherOptions();

if (builder.Environment.IsProduction() && pactEventPublisherOptions.DryRun)
{
    throw new InvalidOperationException(
        "EventBridge:DryRun must be false in Production for Apha.BatchJobs.Pact.Api.");
}

var triggerStoreOptions = builder.Configuration.GetSection("TriggerStore").Get<TriggerStoreOptions>() ?? new TriggerStoreOptions();
var useRedisTriggerStore = builder.Environment.IsProduction()
    || string.Equals(triggerStoreOptions.Provider, "Redis", StringComparison.OrdinalIgnoreCase);

if (useRedisTriggerStore)
{
    var redisConnectionString = !string.IsNullOrWhiteSpace(triggerStoreOptions.RedisConnectionString)
        ? triggerStoreOptions.RedisConnectionString
        : builder.Configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(redisConnectionString)
        || redisConnectionString.Contains("__REPLACE", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "TriggerStore is configured for Redis, but no Redis connection string was provided. Set TriggerStore:RedisConnectionString or ConnectionStrings:Redis.");
    }

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = string.IsNullOrWhiteSpace(triggerStoreOptions.RedisInstanceName)
            ? "pact-trigger-store:"
            : triggerStoreOptions.RedisInstanceName;
    });

    builder.Services.AddSingleton<ITriggerAttemptStore, RedisTriggerAttemptStore>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ITriggerAttemptStore, MemoryTriggerAttemptStore>();
}

builder.Services.AddAWSService<IAmazonEventBridge>();
builder.Services.AddScoped<IEventPublisher, EventBridgePublisher>();
builder.Services.AddScoped<EventBridgeTriggerDispatcher>();
builder.Services.AddScoped<LocalWorkerProcessTriggerDispatcher>();
builder.Services.AddHostedService<LocalWorkerProcessJanitorService>();
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
var canRunHandler = async (
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

        var hasDurableActiveExecution = hasActiveExecution && activeLock is not null;

        var canRun = activeLock is null && !hasDurableActiveExecution;
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
};

app.MapGet("/api/v{version:apiVersion}/batch-jobs/{jobName}/can-run", canRunHandler);

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapPost("/internal/local/batch-jobs/{jobName}/break-glass/release-lock", async (
        string jobName,
        BatchJobsDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var now = DateTime.UtcNow;
        var lockRow = await dbContext.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName && l.IsActive && l.ExpiresAt > now, cancellationToken);

        if (lockRow is null)
        {
            return Results.Ok(new
            {
                released = false,
                reason = "No active lock row found.",
                jobName,
                scope = "local-break-glass"
            });
        }

        dbContext.BatchLocks.Remove(lockRow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            released = true,
            reason = "Active lock row was removed.",
            jobName,
            scope = "local-break-glass",
            releasedLock = new
            {
                lockRow.JobQueueId,
                lockRow.AcquiredAt,
                lockRow.ExpiresAt
            }
        });
    });
}

var statusHandler = async (
    string jobName,
    [FromQuery] string? jobExecutionId,
    [FromQuery] bool? debugView,
    ITriggerAttemptStore triggerAttemptStore,
    IJobExecutionRepository executionRepository,
    ILoggerFactory loggerFactory,
    IHostEnvironment environment,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("StatusEndpoint");

    try
    {
        var hasRequestedJobExecutionId = !string.IsNullOrWhiteSpace(jobExecutionId);
        var requestedJobExecutionIdIsValid = Guid.TryParse(jobExecutionId, out var correlatedExecutionId);

        TriggerAttemptRecord? triggerAttempt = null;

        if (hasRequestedJobExecutionId)
        {
            triggerAttempt = await triggerAttemptStore.GetByJobExecutionIdAsync(jobExecutionId!, cancellationToken);
        }

        var execution = requestedJobExecutionIdIsValid
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
        var queryMode = requestedJobExecutionIdIsValid
            ? "CorrelatedExecutionId"
            : "LatestByJobNameFallback";
        var fallbackReason = requestedJobExecutionIdIsValid
            ? (string?)null
            : hasRequestedJobExecutionId
                ? "InvalidJobExecutionId"
                : "MissingJobExecutionId";

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
            int? workerExitCode = triggerAttempt.WorkerExitCode;

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

            if (!workerProcessExited && workerExitCode.HasValue)
            {
                // Persisted exit code indicates process has already exited.
                workerProcessExited = true;
            }

            string projectedState;
            if (workerProcessExited)
            {
                projectedState = workerExitCode switch
                {
                    0 => "Completed",
                    3 => "Cancelled",
                    4 => "Skipped",
                    _ => string.Equals(triggerAttempt.Status, "Skipped", StringComparison.OrdinalIgnoreCase)
                        ? "Skipped"
                        : string.Equals(triggerAttempt.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                            ? "Cancelled"
                            : "WorkerProcessExited"
                };
            }
            else
            {
                projectedState = now > startupDeadlineUtc
                    ? triggerAttempt.WorkerProcessLaunched
                        ? "WorkerProcessStarted"
                        : "StartFailedTimeout"
                    : triggerAttempt.WorkerProcessLaunched
                        ? "WorkerProcessStarted"
                        : "TriggerAccepted";
            }

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
                triggerStatus = triggerAttempt.Status,
                workerExitCode,
                triggerStore = triggerAttemptStore.StoreName
            };

            isRunning = projectedState != "StartFailedTimeout"
                && projectedState != "WorkerProcessExited"
                && projectedState != "Completed"
                && projectedState != "Skipped"
                && projectedState != "Cancelled";
            sourceOfTruth = "StartupWatchdog";
        }
        else if (execution is not null)
        {
            isRunning = execution.Status == JobStatus.Running || execution.Status == JobStatus.Pending || execution.Status == JobStatus.Retry;
        }

        var businessState = execution is null
            ? (string?)null
            : execution.Status switch
            {
                JobStatus.Pending => "Running",
                JobStatus.Retry => "Running",
                _ => execution.Status.ToString()
            };

        object? diagnostics = null;
        if (debugView == true)
        {
            diagnostics = new
            {
                retry = execution is null ? null : new
                {
                    rawState = execution.Status.ToString(),
                    businessState,
                    execution.RetryAttempts,
                    isInternalOperationalSignal = execution.Status == JobStatus.Retry,
                    note = "Retry is treated as an internal operational signal and should not be surfaced as a primary business UI state."
                },
                triggerStore = triggerAttemptStore.StoreName
            };
        }

        var result = new
        {
            jobName,
            isRunning,
            sourceOfTruth,
            correlatedJobExecutionId,
            queryResolution = new
            {
                mode = queryMode,
                isFallback = string.Equals(queryMode, "LatestByJobNameFallback", StringComparison.Ordinal),
                fallbackReason,
                requestedJobExecutionId = jobExecutionId,
                requestedJobExecutionIdIsValid
            },
            lastExecution = execution is null ? null : new
            {
                execution.ExecutionId,
                execution.JobName,
                execution.JobExecutionId,
                status = execution.Status.ToString(),
                businessState,
                startedAt = execution.StartedAt,
                completedAt = execution.CompletedAt,
                durationSeconds = execution.DurationSeconds,
                recordsProcessed = execution.RecordsProcessed,
                recordsFailed = execution.RecordsFailed,
                errorMessage = execution.ErrorMessage
            },
            startupWatchdog,
            diagnostics
        };

        logger.LogInformation(
            "Status response generated | JobName={JobName} | RequestedJobExecutionId={RequestedJobExecutionId} | CorrelatedJobExecutionId={CorrelatedJobExecutionId} | QueryMode={QueryMode} | SourceOfTruth={SourceOfTruth} | IsRunning={IsRunning}",
            jobName,
            jobExecutionId,
            correlatedJobExecutionId,
            queryMode,
            sourceOfTruth,
            isRunning);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Status request failed | JobName={JobName} | RequestedJobExecutionId={RequestedJobExecutionId}",
            jobName,
            jobExecutionId);

        return Results.Problem($"Failed to fetch status: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
};

app.MapGet("/api/v{version:apiVersion}/batch-jobs/{jobName}/status", statusHandler);

app.Run();
