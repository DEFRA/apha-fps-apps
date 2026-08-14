using System.Data.Common;
using System.Text.Json;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Runs the real, unchanged 12-step YearEnd-DataSetup pipeline (production step classes, resolved
/// via the actual <see cref="YearEndServiceExtensions.AddYearEndJob"/> DI registration so this stays
/// in sync automatically) against live 2025 data in the isolated <c>batchjobs</c> database, inside a
/// single transaction that is ALWAYS rolled back — proving pipeline behaviour correctness while
/// guaranteeing zero committed business-data change. Full design:
/// src/Apha.BatchJobs/docs/fps-year-end-datasetup-rollback-validation-spec-2026-08-14.md
/// </summary>
/// <remarks>
/// Deliberately does NOT call <see cref="YearEndDataSetupService.ExecuteAsync"/> — that method owns
/// and commits its own transaction on success, which would permanently write the 2025-to-2027 copy
/// into <c>batchjobs</c> if every step passed. Instead this test opens its own transaction and
/// invokes each resolved <see cref="IYearEndDataSetupStep"/> directly, in the same order, then
/// always calls ROLLBACK in a <c>finally</c> block regardless of outcome. The steps themselves are
/// 100% unchanged production code, resolved from the real DI registration; only the
/// commit-on-success wrapper is bypassed, deliberately, for safety.
///
/// Gated behind two independent guards, both required: the
/// <c>RUN_YEAR_END_ROLLBACK_VALIDATION</c> environment variable, and a live
/// <c>SELECT current_database()</c> check that the connected database is exactly <c>batchjobs</c> —
/// never trusts the connection string alone. Self-skips otherwise.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndDataSetupRollbackValidationTests : IAsyncLifetime
{
    private const string RequiredDatabaseName = "batchjobs";
    private const string OptInEnvVar = "RUN_YEAR_END_ROLLBACK_VALIDATION";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batchjobs_unconfigured;Username=postgres;Timeout=5";
    private const int SourceFpsYear = 2025;
    private const int TargetFpsYear = 2027;
    private const string AnimalReqSequenceName = "fps.tblanimalreq_indcounter_seq";

    private readonly ITestOutputHelper _output;
    private readonly string _connectionString;
    private string? _skipReason;

    public YearEndDataSetupRollbackValidationTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable(OptInEnvVar), "true", StringComparison.OrdinalIgnoreCase))
        {
            _skipReason = $"Opt-in required: set {OptInEnvVar}=true to run the mutating rollback validation harness.";
            return;
        }

        try
        {
            await using var context = CreateDbContext();
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            var actualDatabase = await context.Database
                .SqlQuery<string>($@"SELECT current_database() AS ""Value""")
                .SingleAsync();

            if (!string.Equals(actualDatabase, RequiredDatabaseName, StringComparison.Ordinal))
            {
                _skipReason = $"Refusing to run: connected database is '{actualDatabase}', expected exactly '{RequiredDatabaseName}'.";
                return;
            }

            var openYears = await context.Database
                .SqlQuery<int>($@"SELECT fpsyear AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus = 'Open'")
                .ToListAsync();

            if (openYears.Count != 1 || openYears[0] != SourceFpsYear)
            {
                _skipReason = $"fps.tblyearmaster does not have exactly one Open year equal to {SourceFpsYear} on this database.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_FullPipelineAgainstBatchjobs_RollsBackWithNoResidualBusinessRows()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var correlationId = $"rollback-validation-{Guid.NewGuid():N}";
        _output.WriteLine($"=== YearEnd rollback validation | database={RequiredDatabaseName} | source={SourceFpsYear} | target={TargetFpsYear} | correlationId={correlationId} ===");

        // Read-only, before the mutating transaction opens: source-year personstatus values that
        // InactiveEmployeeCleanupStep will see unchanged once CopyFpsYearScopedTablesStep propagates
        // them into the target year. An anomaly here means step 10 is expected to fail by design —
        // that is a real data-quality result, not a harness defect.
        var personStatusAnomalies = await CapturePersonStatusAnomaliesAsync();
        if (personStatusAnomalies.Count > 0)
        {
            _output.WriteLine($"WARNING: {personStatusAnomalies.Count} fps.tblwgemployee row(s) in {SourceFpsYear} have a personstatus outside A/a/I/i. InactiveEmployeeCleanupStep is expected to fail by design once it reaches the copied target-year rows.");
            foreach (var anomaly in personStatusAnomalies)
            {
                _output.WriteLine($"  pactid={anomaly.PactId} personstatus='{anomaly.PersonStatus}'");
            }
        }
        else
        {
            _output.WriteLine($"Read-only baseline check: no personstatus anomalies found in {SourceFpsYear} fps.tblwgemployee.");
        }

        var baseline = await CaptureTelemetryAsync("Baseline");
        _output.WriteLine($"Baseline captured | tblyearmaster rows={baseline.YearMasterRows.Count} | matrix tables captured={baseline.Tables.Count} | sequence last_value={baseline.Sequence.LastValue} is_called={baseline.Sequence.IsCalled}");

        string? failedStep = null;
        string? failureMessage = null;
        string? failureDetail = null;
        var finalValidationStatus = "NOT REACHED";
        TelemetrySnapshot? inTransaction = null;

        await using (var provider = BuildStepProvider())
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.OpenConnectionAsync();
            var connection = dbContext.Database.GetDbConnection();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
                var context = new YearEndExecutionContext(correlationId, null, CurrentFpsYear: null, TargetFpsYear: TargetFpsYear);
                var steps = provider.GetServices<IYearEndDataSetupStep>().ToList();

                foreach (var step in steps)
                {
                    var startedAt = DateTime.UtcNow;
                    try
                    {
                        context = await step.ExecuteAsync(context, connection, dbTransaction);
                        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
                        _output.WriteLine($"  [OK]   {step.Name} ({elapsedMs:F0}ms)");

                        if (string.Equals(step.Name, nameof(FinalValidationStep), StringComparison.Ordinal))
                        {
                            finalValidationStatus = "PASSED";
                        }
                    }
                    catch (Exception ex)
                    {
                        failedStep = step.Name;
                        failureMessage = ex.Message;
                        failureDetail = ex.ToString();
                        _output.WriteLine($"  [FAIL] {step.Name}: {ex.Message}");

                        if (string.Equals(step.Name, nameof(FinalValidationStep), StringComparison.Ordinal))
                        {
                            finalValidationStatus = "FAILED";
                        }

                        break;
                    }
                }

                // Capture in-transaction state regardless of outcome — this is what actually exists
                // right before rollback, whether the pipeline completed or stopped partway. A failed
                // step leaves the Postgres transaction in "aborted" state (25P02): any further
                // command on it, including these read-only captures, fails until rollback. That's
                // expected, not a harness bug — skip gracefully rather than let it mask the real
                // failure evidence above or prevent the report from being written.
                try
                {
                    inTransaction = await CaptureTelemetryAsync("InTransaction", connection, dbTransaction);
                }
                catch (Exception telemetryEx)
                {
                    _output.WriteLine($"  (in-transaction telemetry skipped — transaction aborted after step failure: {telemetryEx.Message})");
                }
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        var postRollback = await CaptureTelemetryAsync("PostRollback");

        var pipelineExecution = failedStep is null ? "PASSED" : "FAILED";
        var residualBusinessRows = postRollback.Tables
            .Where(t => t.Role != nameof(YearEndTableRole.YearScopedConfigurationDependency))
            .Sum(t => t.TargetYearCount);
        var sourceCountsByTable = baseline.Tables.ToDictionary(t => (t.Schema, t.Table), t => t.SourceYearCount);
        var sourceYearUnchanged = postRollback.Tables.All(t =>
            sourceCountsByTable.TryGetValue((t.Schema, t.Table), out var baselineSourceCount)
            && baselineSourceCount == t.SourceYearCount);
        var configDependencyRestored = postRollback.Tables
            .Where(t => t.Role == nameof(YearEndTableRole.YearScopedConfigurationDependency))
            .All(t =>
            {
                var baselineTable = baseline.Tables.FirstOrDefault(b => b.Schema == t.Schema && b.Table == t.Table);
                return baselineTable is not null && baselineTable.TargetYearCount == t.TargetYearCount;
            });
        var yearMasterRestored = baseline.YearMasterRows.Count == postRollback.YearMasterRows.Count
            && baseline.YearMasterRows.Zip(postRollback.YearMasterRows, (b, p) =>
                b.FpsYear == p.FpsYear && b.YearStatus == p.YearStatus && b.Active == p.Active).All(x => x);
        var rollbackVerification = residualBusinessRows == 0 && sourceYearUnchanged && configDependencyRestored && yearMasterRestored
            ? "PASSED"
            : "FAILED";
        var sequenceDelta = postRollback.Sequence.LastValue - baseline.Sequence.LastValue;

        var report = new RollbackValidationReport(
            Scenario: $"{SourceFpsYear} -> {TargetFpsYear}",
            Database: RequiredDatabaseName,
            CorrelationId: correlationId,
            CapturedAtUtc: DateTime.UtcNow,
            PipelineExecution: pipelineExecution,
            FailedStep: failedStep,
            FailureMessage: failureMessage,
            FailureDetail: failureDetail,
            FinalValidation: finalValidationStatus,
            RollbackExecuted: "YES",
            RollbackVerification: rollbackVerification,
            ResidualBusinessRows: residualBusinessRows,
            SequenceBefore: baseline.Sequence.LastValue,
            SequenceAfterPipeline: inTransaction?.Sequence.LastValue,
            SequenceAfterRollback: postRollback.Sequence.LastValue,
            SequenceDelta: sequenceDelta,
            PersonStatusAnomaliesInSourceYear: personStatusAnomalies,
            Baseline: baseline,
            InTransaction: inTransaction,
            PostRollback: postRollback);

        var reportPath = WriteReport(report);

        _output.WriteLine("");
        _output.WriteLine("=== SUMMARY ===");
        _output.WriteLine($"PipelineExecution      {report.PipelineExecution}");
        _output.WriteLine($"FailedStep              {report.FailedStep ?? "<none>"}");
        _output.WriteLine($"FinalValidation         {report.FinalValidation}");
        _output.WriteLine($"RollbackExecuted        {report.RollbackExecuted}");
        _output.WriteLine($"RollbackVerification    {report.RollbackVerification}");
        _output.WriteLine($"ResidualBusinessRows    {report.ResidualBusinessRows}");
        _output.WriteLine($"SequenceDelta           {report.SequenceDelta} (before={report.SequenceBefore}, afterPipeline={report.SequenceAfterPipeline?.ToString() ?? "<n/a>"}, afterRollback={report.SequenceAfterRollback})");
        _output.WriteLine($"Report written to       {reportPath}");

        // Hard invariants: whatever the pipeline's business-logic outcome, the harness itself must
        // guarantee these two things. A violation here is a real safety bug in the rollback, not a
        // business-data result — everything else in the report is evidence, not a pass/fail gate.
        Assert.Equal(0, residualBusinessRows);
        Assert.True(sourceYearUnchanged, $"Source-year ({SourceFpsYear}) row counts changed across the transaction — rollback did not fully restore state.");
    }

    private async Task<IReadOnlyList<PersonStatusAnomaly>> CapturePersonStatusAnomaliesAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var dbTransaction = transaction.GetDbTransaction();

        await SetReadOnlyAsync(connection, dbTransaction);

        await using var command = YearEndSqlHelpers.CreateCommand(connection, dbTransaction, @"
            SELECT pactid, personstatus
            FROM fps.tblwgemployee
            WHERE fpsyear = @source_year
              AND UPPER(personstatus) NOT IN ('A', 'I')
            ORDER BY pactid;");
        YearEndSqlHelpers.AddParameter(command, "source_year", SourceFpsYear);

        var results = new List<PersonStatusAnomaly>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                results.Add(new PersonStatusAnomaly(
                    reader.IsDBNull(0) ? "<null>" : reader.GetString(0),
                    reader.IsDBNull(1) ? "<null>" : reader.GetString(1)));
            }
        }

        await transaction.RollbackAsync();
        return results;
    }

    private async Task<TelemetrySnapshot> CaptureTelemetryAsync(string phase)
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var dbTransaction = transaction.GetDbTransaction();

        await SetReadOnlyAsync(connection, dbTransaction);

        var snapshot = await CaptureTelemetryAsync(phase, connection, dbTransaction);
        await transaction.RollbackAsync();
        return snapshot;
    }

    private static async Task SetReadOnlyAsync(DbConnection connection, DbTransaction transaction)
    {
        await using var setReadOnly = connection.CreateCommand();
        setReadOnly.Transaction = transaction;
        setReadOnly.CommandText = "SET TRANSACTION READ ONLY;";
        await setReadOnly.ExecuteNonQueryAsync();
    }

    private static async Task<TelemetrySnapshot> CaptureTelemetryAsync(string phase, DbConnection connection, DbTransaction transaction)
    {
        var yearMasterRows = await CaptureYearMasterAsync(connection, transaction);
        var tables = await CaptureMatrixTelemetryAsync(connection, transaction);
        var sequence = await ReadSequenceSnapshotAsync(connection, transaction);

        return new TelemetrySnapshot(phase, DateTime.UtcNow, yearMasterRows, tables, sequence);
    }

    private static async Task<IReadOnlyList<YearMasterSnapshot>> CaptureYearMasterAsync(DbConnection connection, DbTransaction transaction)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT fpsyear, fpsyearcode, yearstatus, active
            FROM fps.tblyearmaster
            WHERE fpsyear IN (2025, 2026, 2027)
            ORDER BY fpsyear;");

        var results = new List<YearMasterSnapshot>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new YearMasterSnapshot(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetBoolean(3)));
        }

        return results;
    }

    /// <summary>
    /// Matrix-driven — iterates every <see cref="YearEndTableRuleMatrix"/> entry (the same single
    /// source of truth every production step uses) rather than a second hand-maintained table list.
    /// Skips <see cref="YearEndTableRole.GlobalReference"/> entries (no fpsyear/year column, no
    /// target-year row concept).
    /// </summary>
    private static async Task<IReadOnlyList<TableSnapshot>> CaptureMatrixTelemetryAsync(DbConnection connection, DbTransaction transaction)
    {
        var results = new List<TableSnapshot>();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role == YearEndTableRole.GlobalReference)
            {
                continue;
            }

            if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, CancellationToken.None))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, transaction, entry.Schema, entry.TableName);
            if (yearColumn is null)
            {
                continue;
            }

            var sourceCount = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, yearColumn, SourceFpsYear);
            var targetCount = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, yearColumn, TargetFpsYear);

            results.Add(new TableSnapshot(entry.Schema, entry.TableName, entry.Role.ToString(), entry.Action.ToString(), sourceCount, targetCount));
        }

        return results;
    }

    private static async Task<string?> ResolveYearColumnAsync(DbConnection connection, DbTransaction transaction, string schema, string table)
    {
        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "fpsyear", CancellationToken.None))
        {
            return "fpsyear";
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "year", CancellationToken.None))
        {
            return "year";
        }

        return null;
    }

    private static async Task<long> CountByYearAsync(DbConnection connection, DbTransaction transaction, string schema, string table, string yearColumn, int year)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @year;");
        YearEndSqlHelpers.AddParameter(command, "year", year);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, CancellationToken.None);
    }

    private static async Task<SequenceSnapshot> ReadSequenceSnapshotAsync(DbConnection connection, DbTransaction transaction)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT last_value, is_called FROM {AnimalReqSequenceName};");
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new SequenceSnapshot(AnimalReqSequenceName, reader.GetInt64(0), reader.GetBoolean(1));
    }

    /// <summary>
    /// Resolves the real 12-step pipeline via the actual production DI registration
    /// (<see cref="YearEndServiceExtensions.AddYearEndJob"/>) rather than a hand-copied list, so this
    /// harness cannot silently drift out of sync with the registration order if it ever changes.
    /// <see cref="IYearEndDataSetupService"/>/<see cref="IYearEndCutoverService"/> are also registered
    /// by that call but never resolved here, so their <c>IDbContextFactory</c> dependency is never
    /// constructed.
    /// </summary>
    private static ServiceProvider BuildStepProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddYearEndJob();
        return services.BuildServiceProvider();
    }

    private static string WriteReport(RollbackValidationReport report)
    {
        var directory = Environment.GetEnvironmentVariable("YEAR_END_ROLLBACK_REPORT_DIR") ?? AppContext.BaseDirectory;
        Directory.CreateDirectory(directory);
        var fileName = $"yearend-datasetup-{SourceFpsYear}-to-{TargetFpsYear}-rollback-validation-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var path = Path.Combine(directory, fileName);

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);

        return path;
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed record PersonStatusAnomaly(string PactId, string PersonStatus);

    private sealed record YearMasterSnapshot(int FpsYear, string FpsYearCode, string YearStatus, bool Active);

    private sealed record TableSnapshot(string Schema, string Table, string Role, string Action, long SourceYearCount, long TargetYearCount);

    private sealed record SequenceSnapshot(string SequenceName, long LastValue, bool IsCalled);

    private sealed record TelemetrySnapshot(
        string Phase,
        DateTime CapturedAtUtc,
        IReadOnlyList<YearMasterSnapshot> YearMasterRows,
        IReadOnlyList<TableSnapshot> Tables,
        SequenceSnapshot Sequence);

    private sealed record RollbackValidationReport(
        string Scenario,
        string Database,
        string CorrelationId,
        DateTime CapturedAtUtc,
        string PipelineExecution,
        string? FailedStep,
        string? FailureMessage,
        string? FailureDetail,
        string FinalValidation,
        string RollbackExecuted,
        string RollbackVerification,
        long ResidualBusinessRows,
        long SequenceBefore,
        long? SequenceAfterPipeline,
        long SequenceAfterRollback,
        long SequenceDelta,
        IReadOnlyList<PersonStatusAnomaly> PersonStatusAnomaliesInSourceYear,
        TelemetrySnapshot Baseline,
        TelemetrySnapshot? InTransaction,
        TelemetrySnapshot PostRollback);
}
