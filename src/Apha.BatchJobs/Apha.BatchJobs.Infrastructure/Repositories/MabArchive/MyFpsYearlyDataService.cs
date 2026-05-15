using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IMyFpsYearlyDataService.
/// Manages yearly FPS archive data operations (delete, load, refresh).
/// Contract: delete/load/refresh operations are designed to run inside the orchestration transaction
/// provided by the caller so the full year cycle remains atomic.
/// </summary>
public sealed class MyFpsYearlyDataService : IMyFpsYearlyDataService
{
    private readonly BatchJobsDbContext _context;
    private readonly IExecutionYearContext _executionYearContext;
    private readonly ILogger<MyFpsYearlyDataService> _logger;
    private readonly IReadOnlyList<IMabArchiveLoader> _orderedLoaders;
    private readonly IMabArchiveLoader _projectAllLoader;
    private const int ExpectedLoaderCount = 24;

    private static readonly string[] ArchiveDeleteTables =
    {
        // Leaf tables (transaction detail level)
        "mabarchive.my_timecostcalcs",
        "mabarchive.my_monthlyoutput",
        "mabarchive.my_monthlytime",
        "mabarchive.my_projectmonthfinal",
        "mabarchive.my_proj_invoice",
        "mabarchive.my_proj_subcontract",
        "mabarchive.my_tbladditionalcosts",
        "mabarchive.my_tblanimalreq",
        "mabarchive.my_tblcontract",
        "mabarchive.my_tblstaffjob",
        "mabarchive.my_tlkptestreqmt",

        // Dimension tables (setup/reference data)
        "mabarchive.my_testorproduct",
        "mabarchive.my_staff",
        "mabarchive.my_workgroup",
        "mabarchive.my_tblprofitcentre",
        "mabarchive.my_profitcentregrade",
        "mabarchive.my_workgroupgrade",
        "mabarchive.my_tblanimals",

        // Program and project structure
        "mabarchive.my_tlkpprogram",
        "mabarchive.my_tlkpproject",
        "mabarchive.my_tlkpproject_all",

        // Aggregate and year-level tables
        "mabarchive.my_fpsyeartotals",
        "mabarchive.tlkpyear"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="MyFpsYearlyDataService"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="loaders">Registered MABArchive loaders in metadata-defined sequence.</param>
    public MyFpsYearlyDataService(
        BatchJobsDbContext context,
        IExecutionYearContext executionYearContext,
        ILogger<MyFpsYearlyDataService> logger,
        IEnumerable<IMabArchiveLoader> loaders)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var loaderList = (loaders ?? throw new ArgumentNullException(nameof(loaders)))
            .OrderBy(l => l.Sequence)
            .ToList();

        if (loaderList.Count != ExpectedLoaderCount)
        {
            throw new InvalidOperationException($"MABArchive loader registration mismatch. Expected {ExpectedLoaderCount} loaders but found {loaderList.Count}.");
        }

        var duplicateSequences = loaderList
            .GroupBy(l => l.Sequence)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateSequences.Length > 0)
        {
            throw new InvalidOperationException($"MABArchive loaders contain duplicate sequence values: {string.Join(",", duplicateSequences)}.");
        }

        var expectedSequences = Enumerable.Range(1, ExpectedLoaderCount);
        if (!expectedSequences.SequenceEqual(loaderList.Select(l => l.Sequence)))
        {
            throw new InvalidOperationException("MABArchive loader sequence must be contiguous from 1 to 24.");
        }

        _projectAllLoader = loaderList.Single(l => l.Sequence == ExpectedLoaderCount);
        _orderedLoaders = loaderList;
    }

    /// <summary>
    /// Checks whether the supplied year exists in the fiscal year master table.
    /// </summary>
    /// <param name="year">Target year to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the year is available for processing.</returns>
    public async Task<bool> IsYearAvailableAsync(int? year, CancellationToken cancellationToken)
    {
        var targetYear = ResolveYear(year);

        try
        {
            var exists = await _context.Database.SqlQuery<bool>($@"
SELECT EXISTS(
    SELECT 1
    FROM fps.tblyearmaster
    WHERE fpsyear = {targetYear}
) AS ""Value""
").SingleAsync(cancellationToken);

            _logger.LogInformation("Year availability check for {Year}: {Exists}", targetYear, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed year availability check for {Year}", targetYear);
            throw;
        }
    }

    /// <summary>
    /// Deletes archive data for the specified year across archive tables in dependency order.
    /// Implements legacy SQL parity: full year-based wipe of archive dataset for the chosen year.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows deleted.</returns>
    public async Task<int> DeleteYearDataAsync(int? year, CancellationToken cancellationToken)
    {
        var targetYear = ResolveYear(year);

        _logger.LogInformation("Deleting archive data for year {Year} across all archive tables in dependency order (legacy parity scope)", targetYear);

        try
        {
            var totalRowsAffected = 0;

            // Delete order must respect foreign key constraints.
            // Leaf tables first, then parent tables.
            // This list maps to legacy sp_DeleteYearsFPSData coverage per baseline document.
            foreach (var table in ArchiveDeleteTables)
            {
                _logger.LogInformation("Deleting table {TableName} for year {Year}", table, targetYear);
                var deleteSql = $@"
DELETE FROM {table}
WHERE year = @year
";
                var deleteCount = await _context.Database.ExecuteSqlRawAsync(
                    deleteSql,
                    [new NpgsqlParameter("year", targetYear)],
                    cancellationToken);

                totalRowsAffected += deleteCount;
                _logger.LogInformation("Deleted {RowCount} rows from {TableName} for year {Year}", deleteCount, table, targetYear);
            }

            // Special handling for G_tlkpProject: project-based delete matching FPS source projects
            // (not year-based, but included in legacy scope per sp_DeleteYearsFPSData baseline)
            _logger.LogInformation("Deleting table mabarchive.g_tlkpproject using project keys for year {Year}", targetYear);
            var projectDeleteCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.g_tlkpproject
WHERE parentproject IN (
    SELECT DISTINCT parentproject
    FROM fps.tlkpproject
    WHERE fpsyear = {targetYear}
)
", cancellationToken);

            totalRowsAffected += projectDeleteCount;
            _logger.LogInformation("Deleted {RowCount} rows from mabarchive.g_tlkpproject (project-based delete for year {Year})", projectDeleteCount, targetYear);

            _logger.LogInformation("Deleted {TotalRowCount} total rows from archive tables for year {Year} (legacy parity scope)", totalRowsAffected, targetYear);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete archive data for year {Year}", targetYear);
            throw;
        }
    }

    /// <summary>
    /// Loads archive data for the specified year from FPS source tables.
    /// Executes metadata-driven IMabArchiveLoader steps in legacy sequence order.
    /// All inserts are insert-only (no upsert); delete-then-insert is the idempotency mechanism per Assumption A3.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows loaded.</returns>
    public async Task<int> LoadYearDataAsync(int? year, CancellationToken cancellationToken)
    {
        var targetYear = ResolveYear(year);

        _logger.LogInformation("Loading archive data for year {Year} - full sp_AddYearsFPSData fan-out ({LoaderCount} loaders, metadata sequence)", targetYear, _orderedLoaders.Count);

        var currentLoaderNumber = 0;
        var currentLoaderName = "NotStarted";

        try
        {
            var totalRowsAffected = 0;
            _context.ChangeTracker.Clear();

            foreach (var loader in _orderedLoaders)
            {
                _context.ChangeTracker.Clear();
                currentLoaderNumber = loader.Sequence;
                currentLoaderName = loader.Name;

                _logger.LogInformation("[{LoaderNumber}/{TotalLoaders}] Starting {LoaderName} for year {Year}", loader.Sequence, _orderedLoaders.Count, loader.Name, targetYear);

                var rowCount = await loader.LoadAsync(_context, targetYear, cancellationToken);
                totalRowsAffected += rowCount;
                _context.ChangeTracker.Clear();

                _logger.LogInformation("[{LoaderNumber}/{TotalLoaders}] {LoaderName}: {RowCount} rows for year {Year}", loader.Sequence, _orderedLoaders.Count, loader.Name, rowCount, targetYear);
            }

            _logger.LogInformation("LoadYearDataAsync complete: {TotalRowCount} total rows loaded for year {Year}", totalRowsAffected, targetYear);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load archive data for year {Year} while executing loader [{LoaderNumber}/{TotalLoaders}] {LoaderName}",
                targetYear,
                currentLoaderNumber,
                _orderedLoaders.Count,
                currentLoaderName);
            throw;
        }
    }

    /// <summary>
    /// Refreshes only the my_tlkpproject_all table for the specified year.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rows affected in my_tlkpproject_all.</returns>
    public async Task<int> RefreshProjectAllOnlyAsync(int? year, CancellationToken cancellationToken)
    {
        var targetYear = ResolveYear(year);

        _logger.LogInformation("Refreshing project_all cross-reference only for year {Year}", targetYear);

        try
        {
            var deletedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject_all
WHERE year = {targetYear}
", cancellationToken);
            _logger.LogInformation("Deleted {RowCount} rows in my_tlkpproject_all for year {Year} prior to refresh", deletedRows, targetYear);

            var rowsAffected = await _projectAllLoader.LoadAsync(_context, targetYear, cancellationToken);

            _logger.LogInformation("Refreshed {RowCount} rows in my_tlkpproject_all for year {Year}", rowsAffected, targetYear);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh project all for year {Year}", targetYear);
            throw;
        }
    }

    private int ResolveYear(int? explicitYear)
    {
        if (explicitYear.HasValue)
        {
            return explicitYear.Value;
        }

        if (_executionYearContext.FpsYear.HasValue)
        {
            return _executionYearContext.FpsYear.Value;
        }

        throw new InvalidOperationException("Execution year is not set in scoped context and no explicit year was provided.");
    }
}
