namespace Apha.BatchJobs.Application.Jobs.HealthCheck;

/// <summary>
/// Request model for the health check batch job.
/// </summary>
public sealed class HealthCheckJobRequest
{
    /// <summary>
    /// Number of records to process (for testing throughput).
    /// </summary>
    public int RecordCount { get; set; } = 100;

    /// <summary>
    /// Simulate processing delay in milliseconds per record.
    /// </summary>
    public int DelayPerRecordMs { get; set; } = 10;

    /// <summary>
    /// Whether to simulate a failure.
    /// </summary>
    public bool ShouldFail { get; set; } = false;
}
