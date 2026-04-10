using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = ServiceCollectionSetup.CreateDefaultServices();
var serviceProvider = services.BuildServiceProvider();

ILoggerFactory? loggerFactory = null;
IBatchJobFactory? jobFactory = null;

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
    logger.LogInformation("Requested job: {JobName}", jobName);
    
    // Resolve job factory
    jobFactory = serviceProvider.GetRequiredService<IBatchJobFactory>();
    var availableJobs = string.Join(", ", jobFactory.GetAvailableJobs());
    logger.LogInformation("Available jobs: {AvailableJobs}", availableJobs);
    
    // Create and execute the job
    logger.LogInformation("Creating job handler for '{JobName}'...", jobName);
    var job = jobFactory.Create(jobName);
    
    logger.LogInformation("Executing job '{JobName}'...", job.Name);
    await job.ExecuteAsync(CancellationToken.None);
    
    logger.LogInformation("===========================================");
    logger.LogInformation("Batch job '{JobName}' completed successfully", job.Name);
    logger.LogInformation("===========================================");
    Environment.Exit(0);
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
