namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Canonical batch job names used across API, worker, event payloads, and logs.
/// </summary>
public static class BatchJobNames
{
    public const string HealthCheck = "HealthCheck";

    /// <summary>
    /// Runs only the worker's startup clean-up (expired locks, dispatch timeouts) and exits; no
    /// job, job_queue row or lock. Not a registered job_master entry.
    /// </summary>
    public const string Housekeeping = "Housekeeping";
    public const string MabArchive = "MABArchive";
    public const string RecreateSummary = "RecreateSummary";
    public const string YearEndDataSetup = "YearEnd-DataSetup";
    public const string YearEndCutover = "YearEnd-CutOver";

    public const string BulkTestRatesUpdate = "BulkTestRatesUpdate";
    public const string BulkStaffRatesUpdate = "BulkStaffRatesUpdate";
    public const string BulkAnimalRatesUpdate = "BulkAnimalRatesUpdate";

    public const string YearEndLock = "YearEnd";

    public const string MilestoneUpdateNotifications = "MilestoneUpdateNotifications";
}