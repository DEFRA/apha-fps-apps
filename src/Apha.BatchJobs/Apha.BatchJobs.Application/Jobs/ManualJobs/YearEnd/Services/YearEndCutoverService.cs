using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Cutover.
/// Closes the current FPS year, activates the target FPS year, and clears the three PACT-owned
/// staging tables (<see cref="StagingTables"/>) — all in a single transaction. Every mutable
/// precondition (current-year resolution, target-Planned, latest Data Setup Completed) is
/// revalidated from inside this same transaction rather than trusting a pre-transaction read, so
/// the guarantee doesn't lean on the shared YearEnd lock as an implicit second mechanism.
/// </summary>
public sealed class YearEndCutoverService : IYearEndCutoverService
{
    private const string PlannedStatus = "Planned";
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";
    private const string CompletedStatus = "Completed";

    /// <summary>
    /// The three PACT-owned import-validation staging tables Year End CutOver clears as part of
    /// its own transaction (Phase 4 — CutOver Staging Cleanup). None of the three has an
    /// <c>fpsyear</c> column, so clearing is necessarily whole-table, not year-scoped. See
    /// <c>fps-year-end-cutover-contract-trace-and-open-questions-2026-08-15.md</c> for the design
    /// history and the "Option A+" staging-in-use precondition (exclusive table locks, not a
    /// timestamp heuristic).
    /// </summary>
    private static readonly string[] StagingTables =
    {
        "fps.proj_subcontract_staging",
        "fps.tblstagingmonthlyoutput",
        "fps.tblstagingmonthlytime"
    };

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<YearEndCutoverService> _logger;

    public YearEndCutoverService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<YearEndCutoverService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Cutover requires plannedYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        var targetYear = context.TargetFpsYear.Value;

        _logger.LogInformation(
            "YearEndCutover service started | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear}",
            context.CorrelationId,
            targetYear);

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        var closedYear = 0;

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var connection = dbContext.Database.GetDbConnection();
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
                var currentYear = await YearEndYearContextResolver.ResolveCurrentFpsYearAsync(connection, dbTransaction, cancellationToken);
                closedYear = currentYear;

                if (targetYear <= currentYear)
                {
                    throw new InvalidOperationException("plannedYear must be greater than the current Open year for Year End Cutover.");
                }

                var targetState = await GetYearStateForUpdateAsync(connection, dbTransaction, targetYear, cancellationToken);
                if (targetState is null)
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} does not exist in fps.tblyearmaster. Year End Data Setup must complete before cutover.");
                }

                if (!string.Equals(targetState.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} is in status '{targetState.YearStatus}', expected '{PlannedStatus}' before cutover.");
                }

                if (!targetState.Active)
                {
                    throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
                }

                var currentState = await GetYearStateForUpdateAsync(connection, dbTransaction, currentYear, cancellationToken);
                if (currentState is null)
                {
                    throw new InvalidOperationException($"Current year {currentYear} does not exist in fps.tblyearmaster.");
                }

                if (!string.Equals(currentState.YearStatus, OpenStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Current year {currentYear} is in status '{currentState.YearStatus}', expected '{OpenStatus}' before cutover.");
                }

                if (!currentState.Active)
                {
                    throw new InvalidOperationException($"Current year {currentYear} is inactive in fps.tblyearmaster.");
                }

                // Revalidated here, inside the same transaction that performs the mutation —
                // not before BeginTransactionAsync — so this precondition doesn't lean on the
                // shared YearEnd lock as an implicit second guarantee.
                var latestDataSetupStatus = await GetLatestDataSetupStatusAsync(connection, dbTransaction, targetYear, cancellationToken);
                if (!string.Equals(latestDataSetupStatus, CompletedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    var actualStatus = latestDataSetupStatus ?? "None";
                    throw new InvalidOperationException(
                        $"Year End Cutover requires the latest {BatchJobNames.YearEndDataSetup} execution for target year {targetYear} " +
                        $"to be Completed, but found '{actualStatus}'.");
                }

                // "Option A+" staging-in-use precondition: a real database-level guarantee (can we
                // obtain exclusive ownership of these tables right now?) rather than a timestamp
                // heuristic. Fails cleanly, no mutation performed yet, if any table is in use.
                await LockStagingTablesAsync(connection, dbTransaction, cancellationToken);

                await UpdateYearStatusAsync(connection, dbTransaction, currentYear, ClosedStatus, cancellationToken);
                await UpdateYearStatusAsync(connection, dbTransaction, targetYear, OpenStatus, cancellationToken);

                await TruncateStagingTablesAsync(connection, dbTransaction, cancellationToken);

                // Mandatory post-update validation — cheap, and CutOver is exactly where to be
                // paranoid. The shared YearEnd lock only guards against another Year End worker,
                // not against another application, script, or DBA operation touching
                // fps.tblyearmaster, so this is asserted explicitly rather than assumed.
                await ValidateFinalYearStateAsync(connection, dbTransaction, closedYear, targetYear, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });

        _logger.LogInformation(
            "YearEndCutover completed | CorrelationId={CorrelationId} | ClosedYear={ClosedYear} | ActivatedYear={ActivatedYear}",
            context.CorrelationId,
            closedYear,
            targetYear);
    }

    /// <summary>
    /// Latest <c>YearEnd-DataSetup</c> execution status for <paramref name="targetYear"/>, read on
    /// the same connection/transaction as the rest of CutOver (mirrors
    /// <see cref="Apha.BatchJobs.Infrastructure.Repositories.JobExecutionRepository.GetLastExecutionByFpsYearAsync"/>'s
    /// query shape exactly, including its <c>ORDER BY startdatetime DESC</c> — Postgres's default
    /// NULLS FIRST for DESC — since that repository can't participate in this transaction). Returns
    /// <see langword="null"/> when no execution exists for this job/year.
    /// </summary>
    private static async Task<string?> GetLatestDataSetupStatusAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            SELECT s.status
            FROM fps.job_queue q
            JOIN fps.job_master m ON m.jobid = q.jobid
            JOIN fps.job_status s ON s.statusid = q.statusid
            WHERE m.jobname = @jobname AND q.fpsyear = @fpsyear
            ORDER BY q.startdatetime DESC
            LIMIT 1;";

        AddParameter(command, "jobname", BatchJobNames.YearEndDataSetup);
        AddParameter(command, "fpsyear", targetYear);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    /// <summary>
    /// Attempts an immediate, non-blocking exclusive lock on each staging table. Fails cleanly
    /// (no mutation performed anywhere in the transaction yet) if any table is currently in use by
    /// another process, rather than blocking indefinitely on in-flight PACT import activity.
    /// </summary>
    private static async Task LockStagingTablesAsync(
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        foreach (var table in StagingTables)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"LOCK TABLE {table} IN ACCESS EXCLUSIVE MODE NOWAIT;";

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.LockNotAvailable)
            {
                throw new InvalidOperationException(
                    $"Year End Cutover could not acquire an exclusive lock on staging table '{table}' — " +
                    "it is currently in use by another process. Retry once activity has stopped.", ex);
            }
        }
    }

    private static async Task TruncateStagingTablesAsync(
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"TRUNCATE TABLE {string.Join(", ", StagingTables)} RESTART IDENTITY;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Explicit post-update assertion: exactly one Open year, and it is <paramref name="openYear"/>;
    /// <paramref name="closedYear"/> is Closed. Run immediately before commit.
    /// </summary>
    private static async Task ValidateFinalYearStateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int closedYear,
        int openYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT fpsyear FROM fps.tblyearmaster WHERE yearstatus = 'Open';";

        var openYears = new List<int>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                openYears.Add(reader.GetInt32(0));
            }
        }

        if (openYears.Count != 1 || openYears[0] != openYear)
        {
            throw new InvalidOperationException(
                "Year End Cutover post-update validation failed: expected exactly one Open year " +
                $"({openYear}), found [{string.Join(", ", openYears)}].");
        }

        var closedState = await GetYearStateForUpdateAsync(connection, transaction, closedYear, cancellationToken);
        if (closedState is null || !string.Equals(closedState.YearStatus, ClosedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Year End Cutover post-update validation failed: expected year " +
                $"{closedYear} to be Closed, found '{closedState?.YearStatus ?? "missing"}'.");
        }
    }

    private static async Task<YearState?> GetYearStateForUpdateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear
            FOR UPDATE;";

        AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return new YearState(yearStatus, active);
    }

    private static async Task UpdateYearStatusAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        string newStatus,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE fps.tblyearmaster
            SET yearstatus = @yearstatus
            WHERE fpsyear = @fpsyear;";

        AddParameter(command, "yearstatus", newStatus);
        AddParameter(command, "fpsyear", fpsYear);

        var updated = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updated != 1)
        {
            throw new InvalidOperationException(
                $"Expected to update exactly one row for fpsyear {fpsYear}, but updated {updated}.");
        }
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record YearState(string YearStatus, bool Active);
}
