using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;

/// <summary>
/// Shared execution context for RecreateSummaries steps.
/// </summary>
public sealed class RecreateSummariesExecutionContext
{
    /// <summary>
    /// Initializes a new execution context.
    /// </summary>
    /// <param name="dbContext">EF database context.</param>
    /// <param name="connection">Open PostgreSQL connection enrolled in orchestrator transaction.</param>
    /// <param name="fpsYear">The FPS year being processed. All year-scoped steps must filter by this value.</param>
    public RecreateSummariesExecutionContext(BatchJobsDbContext dbContext, NpgsqlConnection connection, int fpsYear)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        FpsYear = fpsYear;
    }

    /// <summary>EF database context for future LINQ-based step implementations.</summary>
    public BatchJobsDbContext DbContext { get; }

    /// <summary>Open PostgreSQL connection enrolled in orchestrator transaction.</summary>
    public NpgsqlConnection Connection { get; }

    /// <summary>
    /// The FPS year being processed. In the legacy SQL era each year had its own database;
    /// in PostgreSQL all years share one database with <c>fpsyear</c> as the discriminator.
    /// Every step that touches year-scoped tables must filter by this value.
    /// </summary>
    public int FpsYear { get; }
}
