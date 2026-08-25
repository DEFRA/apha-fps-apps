using System.ComponentModel.DataAnnotations;

namespace Apha.BatchJobs.Worker.Configuration;

// DbCommandTimeoutSeconds/LockTimeoutSeconds are intentionally absent: those stay in BatchJobSettings where 0 means unbounded.
public sealed class BatchRuntimeOptions
{
    public const string SectionName = "BatchJobs";

    // Seconds; upper bound for host.StopAsync() before ECS SIGTERM forced-stop fires.
    [Range(1, int.MaxValue)]
    public int GracefulShutdownWindowSeconds { get; init; } = 25;

    // Seconds; outer wall-clock cap for one RunAsync call. Distinct from JobTimeout where 0 = unbounded.
    [Range(1, int.MaxValue)]
    public int WorkerOverallTimeoutSeconds { get; init; } = 3600;
}
