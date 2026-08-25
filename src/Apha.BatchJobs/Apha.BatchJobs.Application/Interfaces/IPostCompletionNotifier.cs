using Apha.BatchJobs.Application.Orchestration;

namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Invoked by the Orchestrator after a job is durably marked Completed and its lock released.
/// Implementations are best-effort; failures must never propagate into the job lifecycle.
/// </summary>
public interface IPostCompletionNotifier
{
    Task NotifyAsync(BatchJobCompletionContext context, CancellationToken cancellationToken);
}
