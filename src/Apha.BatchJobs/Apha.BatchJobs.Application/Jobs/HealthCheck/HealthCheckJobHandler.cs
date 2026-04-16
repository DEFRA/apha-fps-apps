using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.HealthCheck;

/// <summary>
/// Health check job handler for testing the batch jobs framework.
/// This job validates logging, database connectivity, and execution tracking.
/// </summary>
public sealed class HealthCheckJobHandler : IBatchJob
{
    private readonly ILogger<HealthCheckJobHandler> _logger;

    /// <summary>
    /// Name of this job.
    /// </summary>
    public string Name => "HealthCheck";

    /// <summary>
    /// Explicit idempotency strategy declaration for this job.
    /// HealthCheck is read/validate-only and produces no mutable side effects.
    /// </summary>
    public string IdempotencyStrategy => "NoWriteValidation";

    /// <summary>
    /// Initializes a new instance of the HealthCheckJobHandler.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public HealthCheckJobHandler(ILogger<HealthCheckJobHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the health check job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== HealthCheck Job Started ===");
        _logger.LogInformation("Job: {JobName}", Name);
        _logger.LogInformation("Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}", DateTime.UtcNow);
        _logger.LogInformation("ProcessId: {ProcessId}", Environment.ProcessId);

        try
        {
            // Phase 1: Validate configuration
            _logger.LogInformation("Phase 1: Validating configuration...");
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not Set";
            _logger.LogInformation("  Environment: {Environment}", envName);
            _logger.LogInformation("  .NET Version: {DotNetVersion}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            _logger.LogInformation("  OS: {OS}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);

            // Phase 2: Simulate work
            _logger.LogInformation("Phase 2: Processing records...");
            var recordCount = 50;
            var successCount = 0;

            for (int i = 1; i <= recordCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation requested at record {RecordNumber}", i);
                    break;
                }

                successCount++;
                
                if (i % 10 == 0)
                {
                    _logger.LogInformation("  Processed {RecordsProcessed}/{TotalRecords} records", i, recordCount);
                }

                // Simulate work delay
                await Task.Delay(50, cancellationToken);
            }

            // Phase 3: Validate database connectivity
            _logger.LogInformation("Phase 3: Validating database connectivity...");
            _logger.LogInformation("  Database operations will be tested in execution repository write");

            // Phase 4: Report results
            _logger.LogInformation("Phase 4: Job completion report");
            _logger.LogInformation("  Records Processed: {RecordsProcessed}", successCount);
            _logger.LogInformation("  Success Rate: {SuccessRate:P}", (double)successCount / recordCount);
            _logger.LogInformation("  Completed At: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}", DateTime.UtcNow);
            _logger.LogInformation("=== HealthCheck Job Completed Successfully ===");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "HealthCheck job was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthCheck job failed with error: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
