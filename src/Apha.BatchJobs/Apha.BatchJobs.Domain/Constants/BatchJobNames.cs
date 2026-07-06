namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Canonical batch job names used across API, worker, event payloads, and logs.
/// </summary>
public static class BatchJobNames
{
    public const string HealthCheck = "HealthCheck";
    public const string MabArchive = "MABArchive";
    public const string RecreateSummary = "RecreateSummary";
    public const string FecProcess = "FECProcess";
    public const string YearEndDataSetup = "YearEndDataSetup";
    public const string YearEndCutover = "YearEndCutover";

    public const string YearEndLock = "YearEnd";
}