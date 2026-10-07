namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Default CloudWatch <c>ErrorType</c> marker tokens written as <c>[{ErrorType}]</c> in worker logs.
/// <see cref="General"/> and <see cref="Database"/> must match the existing BatchJobs metric filter
/// patterns exactly, or failures will not raise alarms. Overridable via <c>ExceptionTypes</c> configuration.
/// </summary>
public static class BatchExceptionMarkers
{
    /// <summary>Unclassified, configuration, business, and startup failures.</summary>
    public const string General = "FPSAPPS.EXCEPTION.BATCHJOBS.GENERAL";

    /// <summary>PostgreSQL / EF Core / connectivity failures.</summary>
    public const string Database = "FPSAPPS.EXCEPTION.BATCHJOBS.DB";

    /// <summary><see cref="Exceptions.JobValidationException"/>. No metric filter matches this marker.</summary>
    public const string Validation = "FPSAPPS.EXCEPTION.BATCHJOBS.VALIDATION";

    /// <summary>Lock acquisition and lease failures. No metric filter matches this marker.</summary>
    public const string Concurrency = "FPSAPPS.EXCEPTION.BATCHJOBS.CONCURRENCY";
}
