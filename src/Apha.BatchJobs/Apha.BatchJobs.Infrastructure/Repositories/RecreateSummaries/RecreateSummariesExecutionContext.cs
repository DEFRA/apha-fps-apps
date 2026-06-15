using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Shared execution context for RecreateSummaries steps.
/// </summary>
public sealed class RecreateSummariesExecutionContext
{
    /// <summary>
    /// Initializes a new execution context.
    /// </summary>
    public RecreateSummariesExecutionContext(BatchJobsDbContext dbContext, NpgsqlConnection connection)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>EF database context for future LINQ-based step implementations.</summary>
    public BatchJobsDbContext DbContext { get; }

    /// <summary>Open PostgreSQL connection enrolled in orchestrator transaction.</summary>
    public NpgsqlConnection Connection { get; }
}
