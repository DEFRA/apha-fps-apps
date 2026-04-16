using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Worker;
using Apha.BatchJobs.Worker.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var startedAt = DateTime.UtcNow;
const int ExitCodeSuccess = 0;
const int ExitCodeBusinessFailure = 1;
const int ExitCodeConfigurationError = 2;
const int ExitCodeCancelled = 3;
const int ExitCodeSkipped = 4;
const int ExitCodeDependencyOutage = 5;

// ECS SIGTERM → forced-stop window is typically 30 s.
// We allow 25 s for graceful cleanup before the host forces termination.
const int GracefulShutdownWindowSeconds = 25;

if (builder.Environment.IsEnvironment("local"))
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("Logs/BatchJobs.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}
else
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .UseStructuredConsoleLogging()
        .CreateLogger();
}

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.ConfigureServices();

using var host = builder.Build();
var serviceProvider = host.Services;

ILoggerFactory? loggerFactory = null;
string failureCategory = "BusinessFailure";
string runOutcome = "Failed";
string? requestedJobName = null;
string requestedRunMode = "AdHoc";
string? capturedRunId = null;
int? capturedExecutionId = null;
var exitCode = ExitCodeBusinessFailure;

try
{
    await host.StartAsync();

    loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("BatchJobs.Startup");
    var hostLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
    
    logger.LogInformation("===========================================");
    logger.LogInformation("Batch Jobs Worker - Starting");
    logger.LogInformation("===========================================");
    logger.LogInformation("Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}", startedAt);
    logger.LogInformation("ProcessId: {ProcessId}", Environment.ProcessId);
    logger.LogInformation("Environment: {EnvironmentName}", builder.Environment.EnvironmentName);
    
    // Get job name from args or environment variable
    var jobName = args.Length > 0 ? args[0] : (Environment.GetEnvironmentVariable("BATCH_JOB_NAME") ?? "HealthCheck");
    var runModeEnv = Environment.GetEnvironmentVariable("BATCH_RUN_MODE") ?? "AdHoc";
    var runMode = Enum.TryParse<RunMode>(runModeEnv, ignoreCase: true, out var parsedMode) ? parsedMode : RunMode.AdHoc;
    requestedJobName = jobName;
    requestedRunMode = runMode.ToString();

    logger.LogInformation("Requested job: {JobName} | RunMode: {RunMode}", jobName, runMode);

    // Link host shutdown with a bounded graceful-window timeout.
    // This ensures the job is cancelled well within the ECS SIGTERM → forced-stop window.
    using var shutdownWindowCts = new CancellationTokenSource(TimeSpan.FromSeconds(GracefulShutdownWindowSeconds));
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        hostLifetime.ApplicationStopping,
        shutdownWindowCts.Token);

    if (hostLifetime.ApplicationStopping.IsCancellationRequested)
    {
        logger.LogWarning(
            "Host stopping signal was already set before job start — skipping execution | JobName={JobName} | GracefulWindowSeconds={GracefulWindowSeconds}",
            jobName, GracefulShutdownWindowSeconds);
        exitCode = ExitCodeCancelled;
        failureCategory = "Cancellation";
        runOutcome = "Cancelled";
    }
    else
    {
        // Run through orchestrator (handles lock, execution records, and job execution)
        var orchestrator = serviceProvider.GetRequiredService<IJobOrchestrator>();
        var result = await orchestrator.RunAsync(jobName, runMode, linkedCts.Token);

        capturedRunId = result.RunId;
        capturedExecutionId = result.ExecutionId;

        logger.LogInformation("===========================================");
        logger.LogInformation(
            "Job '{JobName}' finished | Status={Status} | RunId={RunId} | ExecutionId={ExecutionId}",
            result.JobName, result.Status, result.RunId, result.ExecutionId);
        var computedExitCode = result.Status == JobStatus.Skipped ? ExitCodeSkipped : ExitCodeSuccess;
        logger.LogInformation("===========================================");

        // Exit 4 = skipped (lock already held) — not a failure
        exitCode = computedExitCode;
        if (result.Status == JobStatus.Skipped)
        {
            failureCategory = "LockContentionSkip";
            runOutcome = "Skipped";
        }
        else
        {
            failureCategory = "None";
            runOutcome = "Succeeded";
        }
    }
}
catch (InvalidOperationException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    logger?.LogError(ex, "Job factory error: {ErrorMessage}", ex.Message);
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    exitCode = ExitCodeConfigurationError;
    failureCategory = "ConfigurationError";
    runOutcome = "Failed";
}
catch (OperationCanceledException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var remainingWindowMs = Math.Max(0, (int)(GracefulShutdownWindowSeconds * 1000 - (DateTime.UtcNow - startedAt).TotalMilliseconds));
    logger?.LogWarning(ex,
        "Job was cancelled | JobName={JobName} | RunId={RunId} | RemainingShutdownWindowMs={RemainingWindowMs}",
        requestedJobName ?? "Unknown", capturedRunId ?? "N/A", remainingWindowMs);
    Console.Error.WriteLine($"CANCELLED: Job execution was cancelled");
    exitCode = ExitCodeCancelled;
    failureCategory = "Cancellation";
    runOutcome = "Cancelled";
}
catch (Exception ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    if (IsDependencyOutage(ex))
    {
        logger?.LogError(ex, "Dependency outage detected: {ErrorMessage}", ex.Message);
        exitCode = ExitCodeDependencyOutage;
        failureCategory = "DependencyOutage";
    }
    else
    {
        logger?.LogError(ex, "Unhandled business/runtime exception: {ErrorMessage}", ex.Message);
        exitCode = ExitCodeBusinessFailure;
        failureCategory = "BusinessFailure";
    }

    Console.Error.WriteLine($"FATAL ERROR: {ex}");
    runOutcome = "Failed";
}
finally
{
    try
    {
        var logger = loggerFactory?.CreateLogger("BatchJobs.Summary");
        logger?.LogInformation(
            "Run completed | Outcome={Outcome} | FailureCategory={FailureCategory} | ExitCode={ExitCode} | JobName={JobName} | RunId={RunId} | ExecutionId={ExecutionId} | RunMode={RunMode} | TotalDurationMs={DurationMs}",
            runOutcome,
            failureCategory,
            exitCode,
            requestedJobName ?? "Unknown",
            capturedRunId ?? "N/A",
            capturedExecutionId?.ToString() ?? "N/A",
            requestedRunMode,
            (DateTime.UtcNow - startedAt).TotalMilliseconds);
    }
    catch
    {
        // Preserve original exit behavior if summary logging itself fails.
    }

    try
    {
        await host.StopAsync();
    }
    catch (Exception ex)
    {
        var logger = loggerFactory?.CreateLogger("BatchJobs.Shutdown");
        logger?.LogWarning(ex, "Host stop encountered an issue during shutdown");
    }

    Log.CloseAndFlush();
}

return exitCode;

static bool IsDependencyOutage(Exception ex)
{
    for (Exception? current = ex; current != null; current = current.InnerException)
    {
        if (current is NpgsqlException || current is TimeoutException || current is DbUpdateException)
            return true;
    }

    return false;
}
