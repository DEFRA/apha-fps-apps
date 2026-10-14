namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Default CloudWatch <c>ErrorType</c> marker tokens written as <c>[{ErrorType}]</c> in worker logs.
/// <see cref="General"/> and <see cref="Database"/> must match the existing BatchJobs metric filter
/// patterns exactly, or failures will not raise alarms. Overridable via <c>ExceptionTypes</c> configuration.
/// </summary>
public static class BatchExceptionMarkers
{
    /// <summary>Unclassified, configuration, validation, lock, business, and startup failures.</summary>
    public const string General = "FPSAPPS.EXCEPTION.BATCHJOBS.GENERAL";

    /// <summary>PostgreSQL / EF Core / connectivity failures.</summary>
    public const string Database = "FPSAPPS.EXCEPTION.BATCHJOBS.DB";
}
