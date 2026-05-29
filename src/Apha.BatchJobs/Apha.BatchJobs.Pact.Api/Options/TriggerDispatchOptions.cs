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

    public string? WorkingDirectory { get; init; }

    public bool WaitForDebuggerAttach { get; init; }

    public int DebuggerAttachTimeoutSeconds { get; init; } = 120;

    public bool BreakOnStartAfterAttach { get; init; } = true;
}
