using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Data.Common;

namespace Apha.BatchJobs.Infrastructure.YearEnd.Repositories;

/// <summary>
/// Executes the year-status transition for Year End Cutover inside a single transaction: closes the
/// current year, activates the target year, and clears the three PACT-owned staging tables — all
/// atomically. Every mutable precondition (target-Planned, current-Open, latest Data Setup Completed)
/// is revalidated from inside this same transaction rather than trusting a pre-transaction read, so
/// the guarantee doesn't lean on the shared YearEnd lock as an implicit second mechanism.
/// </summary>
public sealed class YearEndCutoverRepository : IYearEndCutoverRepository
{
    private const string PlannedStatus = "Planned";
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";
    private const string CompletedStatus = "Completed";

    /// <summary>
    /// The three PACT-owned import-validation staging tables Cutover clears as part of its own
    /// transaction. None has an <c>fpsyear</c> column, so clearing is necessarily whole-table, not
    /// year-scoped.
    /// </summary>
    private static readonly string[] StagingTables =
    {
        "fps.proj_subcontract_staging",
        "fps.tblstagingmonthlyoutput",
        "fps.tblstagingmonthlytime"
    };

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;

    public YearEndCutoverRepository(IDbContextFactory<BatchJobsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task ExecuteCutoverAsync(int currentYear, int targetYear, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var connection = dbContext.Database.GetDbConnection();
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
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

                // Revalidated here, inside the same transaction that performs the mutation — not
                // before BeginTransactionAsync — so this precondition doesn't lean on the shared
                // YearEnd lock as an implicit second guarantee. The service layer's own
                // pre-transaction check is a fast-fail only, not the correctness guarantee.
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
                // paranoid. The shared YearEnd lock only guards against another Year End worker, not
                // against another application, script, or DBA operation touching fps.tblyearmaster.
                await ValidateFinalYearStateAsync(connection, dbTransaction, currentYear, targetYear, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }

    /// <summary>
    /// Latest <see cref="BatchJobNames.YearEndDataSetup"/> execution status for
    /// <paramref name="targetYear"/>, read on the same connection/transaction as the rest of Cutover
    /// (mirrors <c>JobExecutionRepository.GetLastExecutionByFpsYearAsync</c>'s query shape exactly,
    /// including its <c>ORDER BY startdatetime DESC</c> — that repository can't participate in this
    /// transaction). Returns <see langword="null"/> when no execution exists for this job/year.
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
    /// Attempts an immediate, non-blocking exclusive lock on each staging table. Fails cleanly (no
    /// mutation performed anywhere in the transaction yet) if any table is currently in use by
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
    /// Explicit post-update assertion: exactly one Open year, and it is <paramref name="targetYear"/>;
    /// <paramref name="closedYear"/> is Closed. Run immediately before commit.
    /// </summary>
    private static async Task ValidateFinalYearStateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int closedYear,
        int targetYear,
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

        if (openYears.Count != 1 || openYears[0] != targetYear)
        {
            throw new InvalidOperationException(
                "Year End Cutover post-update validation failed: expected exactly one Open year " +
                $"({targetYear}), found [{string.Join(", ", openYears)}].");
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
