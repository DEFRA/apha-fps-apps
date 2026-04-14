using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = ServiceCollectionSetup.CreateDefaultServices();
var serviceProvider = services.BuildServiceProvider();

ILoggerFactory? loggerFactory = null;

try
{
    loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("BatchJobs.Startup");
    
    logger.LogInformation("===========================================");
    logger.LogInformation("Batch Jobs Worker - Starting");
    logger.LogInformation("===========================================");
    logger.LogInformation("Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}", DateTime.UtcNow);
    logger.LogInformation("ProcessId: {ProcessId}", Environment.ProcessId);
    logger.LogInformation("Total services registered: {ServiceCount}", services.Count);
    
    // Get job name from args or environment variable
    var jobName = args.Length > 0 ? args[0] : (Environment.GetEnvironmentVariable("BATCH_JOB_NAME") ?? "HealthCheck");
    var runModeEnv = Environment.GetEnvironmentVariable("BATCH_RUN_MODE") ?? "AdHoc";
    var runMode = Enum.TryParse<RunMode>(runModeEnv, ignoreCase: true, out var parsedMode) ? parsedMode : RunMode.AdHoc;

    logger.LogInformation("Requested job: {JobName} | RunMode: {RunMode}", jobName, runMode);

    // Run through orchestrator (handles lock, execution records, and job execution)
    var orchestrator = serviceProvider.GetRequiredService<IJobOrchestrator>();
    var result = await orchestrator.RunAsync(jobName, runMode, CancellationToken.None);

    logger.LogInformation("===========================================");
    logger.LogInformation("Job '{JobName}' finished | Status={Status} | RunId={RunId}",
        result.JobName, result.Status, result.RunId);
    logger.LogInformation("===========================================");

    // Exit 4 = skipped (lock already held) — not a failure
    var exitCode = result.Status == JobStatus.Skipped ? 4 : 0;
    Environment.Exit(exitCode);
}
catch (InvalidOperationException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    logger?.LogError(ex, "Job factory error: {ErrorMessage}", ex.Message);
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Environment.Exit(2);
}
catch (OperationCanceledException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    logger?.LogWarning(ex, "Job was cancelled");
    Console.Error.WriteLine($"CANCELLED: Job execution was cancelled");
    Environment.Exit(3);
}
catch (Exception ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    logger?.LogError(ex, "Unhandled exception: {ErrorMessage}", ex.Message);
    Console.Error.WriteLine($"FATAL ERROR: {ex}");
    Environment.Exit(1);
}
finally
{
    await serviceProvider.DisposeAsync();
}
