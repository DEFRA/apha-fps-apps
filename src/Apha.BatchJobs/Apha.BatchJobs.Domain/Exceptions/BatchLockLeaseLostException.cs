namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown by the heartbeat when a held <c>job_lock</c> lease is definitively lost mid-execution —
/// either <c>TryRenewLockAsync</c> returned <c>false</c> (an expired lease can never be renewed
/// by its own owner, per the non-renewable-once-expired guarantee) or renewal kept failing until
/// the locally-tracked lease boundary passed without a successful renewal. Inherits
/// <see cref="JobLockException"/> so <c>BatchFailureClassifier</c>'s existing
/// <c>JobLockException</c> case (exit code <see cref="Constants.BatchExitCodes.LockFailure"/>,
/// category Concurrency) applies automatically via C#'s subclass-matching type patterns, while
/// remaining a distinct type so logs/messages can say "lease expired mid-run" rather than
/// "already running".
/// </summary>
public sealed class BatchLockLeaseLostException : JobLockException
{
    public BatchLockLeaseLostException(string message) : base(message) { }

    public BatchLockLeaseLostException(string message, Exception innerException)
        : base(message, innerException) { }
}
