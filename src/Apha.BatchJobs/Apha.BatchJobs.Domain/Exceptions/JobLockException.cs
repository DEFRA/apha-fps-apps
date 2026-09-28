namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>Thrown when the distributed lock for a batch job cannot be acquired (e.g. the job is already running or the lock table is unavailable). Maps to exit code <see cref="Constants.BatchExitCodes.LockFailure"/> (30). Not sealed — <see cref="BatchLockLeaseLostException"/> inherits from it so the same classification applies to a lease lost mid-execution, not just a failed acquisition.</summary>
public class JobLockException : Exception
{
    public JobLockException(string message) : base(message) { }

    public JobLockException(string message, Exception innerException)
        : base(message, innerException) { }
}
