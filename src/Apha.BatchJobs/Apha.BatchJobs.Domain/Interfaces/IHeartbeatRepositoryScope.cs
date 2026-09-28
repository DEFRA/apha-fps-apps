namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// A short-lived, independent pair of lock/execution repositories backed by their own DbContext,
/// used by <c>JobOrchestrator</c>'s heartbeat loop so it never shares a DbContext with whatever
/// the running job itself is doing concurrently on the ambient scoped context — EF Core's
/// DbContext is not thread-safe, and the heartbeat runs alongside the job, not sequentially with
/// it. Dispose (or <c>await using</c>) to release the underlying DbContext once the heartbeat
/// loop ends.
/// </summary>
public interface IHeartbeatRepositoryScope : IAsyncDisposable
{
    IBatchLockRepository LockRepository { get; }

    IJobExecutionRepository ExecutionRepository { get; }
}

/// <summary>Creates a new <see cref="IHeartbeatRepositoryScope"/> on demand.</summary>
public interface IHeartbeatRepositoryScopeFactory
{
    IHeartbeatRepositoryScope Create();
}
