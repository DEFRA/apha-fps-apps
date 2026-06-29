using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>
/// Custom EF Core execution strategy that retries on transient Npgsql errors
/// (using <see cref="NpgsqlException.IsTransient"/>) but explicitly excludes
/// disk-full errors (SQLSTATE 53100). Retrying a disk-full condition would
/// re-execute the same heavy query, generating additional temp spill and
/// exhausting storage faster.
/// </summary>
internal sealed class BatchJobsRetryStrategy : ExecutionStrategy
{
    // PostgreSQL error class 53 = Insufficient Resources
    // 53100 = disk_full: "could not write to file base/pgsql_tmp/...: No space left on device"
    private const string DiskFullSqlState = "53100";

    public BatchJobsRetryStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay)
        : base(dependencies, maxRetryCount, maxRetryDelay)
    {
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        // Never retry disk-full: each retry re-executes the heavy query, making spill worse.
        if (exception is PostgresException pgEx && pgEx.SqlState == DiskFullSqlState)
            return false;

        // Delegate to Npgsql's built-in transient-error detection for everything else.
        if (exception is NpgsqlException npEx)
            return npEx.IsTransient;

        return false;
    }
}
