namespace Apha.BatchJobs.Pact.Api.Options;

public sealed class TriggerDispatchOptions
{
    // Allowed values: EventBridge (default), LocalProcess
    public string Mode { get; init; } = "EventBridge";

    public LocalWorkerOptions LocalWorker { get; init; } = new();
}

public sealed class LocalWorkerOptions
{
    public string DotnetExecutable { get; init; } = "dotnet";

    // Relative to PACT API content root by default.
    public string WorkerProjectPath { get; init; } = "..\\Apha.BatchJobs.Worker\\Apha.BatchJobs.Worker.csproj";

    // Optional environment name for spawned worker process.
    // If omitted, PACT API environment name is reused.
    public string? WorkerEnvironmentName { get; init; }

    public string? WorkingDirectory { get; init; }

    public bool WaitForDebuggerAttach { get; init; }

    public int DebuggerAttachTimeoutSeconds { get; init; } = 120;

    public bool BreakOnStartAfterAttach { get; init; } = true;

    // When false, noisy EF SQL command text is omitted from per-run local worker logs.
    public bool IncludeSqlCommandLogs { get; init; }

    // Enables background cleanup of stale local worker processes tracked by metadata files.
    public bool EnableOrphanProcessReaper { get; init; } = true;

    // Reaper loop interval.
    public int ReaperIntervalSeconds { get; init; } = 30;

    // Maximum allowed age before a tracked worker process is treated as stale.
    public int MaxWorkerLifetimeMinutes { get; init; } = 30;
}
