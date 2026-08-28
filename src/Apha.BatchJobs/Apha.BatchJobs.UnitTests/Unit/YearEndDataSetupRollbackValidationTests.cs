using System.Data.Common;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Proves the Year End main-port Phase 7A atomicity guarantee against live data in the isolated
/// <c>batchjobs</c> database: the real, unchanged 11 production Data Setup steps (resolved via the
/// actual <see cref="BatchPersistenceServiceExtensions.AddBatchPersistence"/> +
/// <see cref="YearEndServiceExtensions.AddYearEndJob"/> DI registration, so this cannot silently drift
/// out of sync) either all commit together, or a failure at any point rolls back everything —
/// including mutations from steps that had already completed successfully before the failure.
/// </summary>
/// <remarks>
/// <para>
/// This is now a genuine in-process-transaction-rollback harness — <see cref="IYearEndDataSetupTransactionManager"/>
/// (Phase 7A) wraps the whole run in one transaction on a shared, scoped <c>BatchJobsDbContext</c>, so
/// an injected failure partway through the pipeline is expected to roll back every prior step's
/// mutations too, not just leave them committed. That real transaction is exactly what this harness
/// exercises: failures are injected after a real prefix of steps has executed for real (not faked), so
/// a rollback failure here would be a genuine regression in the Phase 7A guarantee, not a test artifact.
/// </para>
/// <para>
/// Gated behind two independent guards, both required: the <c>RUN_YEAR_END_ROLLBACK_VALIDATION</c>
/// environment variable, and a live <c>SELECT current_database()</c> check that the connected database
/// is exactly <c>batchjobs</c> — never trusts the connection string alone. Self-skips otherwise. The
/// source (Open) year and target (Planned-to-be-created) year are resolved dynamically from
/// <c>fps.tblyearmaster</c> at run time (source = the current Open year, target = source + 1) rather
/// than hardcoded, so this harness stays valid across whatever years the isolated database is seeded
/// with.
/// </para>
/// <para>
/// The one scenario that still legitimately commits (all steps succeed) has no in-process transaction
/// to roll back by design — it uses the same matrix-driven cleanup approach as before Phase 7A to
/// restore baseline afterward, since a fully successful run is a real, intentional commit, not a
/// failure.
/// </para>
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndDataSetupRollbackValidationTests : IAsyncLifetime
{
    private const string RequiredDatabaseName = "batchjobs";
    private const string OptInEnvVar = "RUN_YEAR_END_ROLLBACK_VALIDATION";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batchjobs_unconfigured;Username=postgres;Timeout=5";

    private readonly ITestOutputHelper _output;
    private readonly string _connectionString;
    private string? _skipReason;
    private int _sourceFpsYear;
    private int _targetFpsYear;

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
            _skipReason = $"Opt-in required: set {OptInEnvVar}=true to run the mutating Data Setup validation harness.";
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

            if (openYears.Count != 1)
            {
                _skipReason = "fps.tblyearmaster does not have exactly one Open year on this database.";
                return;
            }

            _sourceFpsYear = openYears[0];
            _targetFpsYear = _sourceFpsYear + 1;

            var targetAlreadyExists = await context.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblyearmaster WHERE fpsyear = {_targetFpsYear}")
                .SingleAsync() > 0;

            if (targetAlreadyExists)
            {
                _skipReason = $"Target year {_targetFpsYear} already exists in fps.tblyearmaster — this harness expects to create it itself (and, for the failure scenarios, prove it does NOT survive rollback).";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_WhenFailureInjectedEarly_RollsBackEveryPriorStepsMutations()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");
        // Through CreatePlannedYearStep only — proves even a single already-committed-looking insert
        // (the target year's own fps.tblyearmaster row) is rolled back.
        await RunInjectedFailureScenarioAsync("Early", stepNameToRunThrough: nameof(CreatePlannedYearStep));
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenFailureInjectedMidway_RollsBackEveryPriorStepsMutations()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");
        // Through ProjectFinancialResetStep — planned year created, all copyable tables copied, first
        // reset phase applied.
        await RunInjectedFailureScenarioAsync("Midway", stepNameToRunThrough: nameof(ProjectFinancialResetStep));
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenFailureInjectedLate_BeforeFinalValidation_RollsBackEveryPriorStepsMutations()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");
        // Through ValidateTargetYearEmptyTablesStep — everything except FinalValidationStep itself has
        // run. The strongest proof: almost the entire pipeline's worth of mutations must still roll
        // back, even though this particular step (Phase 7B) no longer mutates anything itself.
        await RunInjectedFailureScenarioAsync("Late", stepNameToRunThrough: nameof(ValidateTargetYearEmptyTablesStep));
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenAllStepsSucceed_CommitsTheCompleteResultExactlyOnce()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var correlationId = $"datasetup-success-{Guid.NewGuid():N}";
        _output.WriteLine($"=== YearEnd Data Setup full-success validation | source={_sourceFpsYear} | target={_targetFpsYear} | correlationId={correlationId} ===");

        var baseline = await CaptureTelemetryAsync("Baseline");
        AssertBaselineIsClean(baseline);

        try
        {
            await using var provider = BuildRealServiceProvider();
            var service = provider.GetRequiredService<IYearEndDataSetupService>();
            var context = new YearEndExecutionContext(correlationId, null, CurrentFpsYear: _sourceFpsYear, TargetFpsYear: _targetFpsYear);

            await service.ExecuteAsync(context, CancellationToken.None);
            _output.WriteLine("Full pipeline completed successfully — all 11 steps committed together.");

            var postCommit = await CaptureTelemetryAsync("PostCommit");
            var committedTargetRows = postCommit.Tables.Sum(t => t.TargetYearCount);
            _output.WriteLine($"Target-year rows present after commit: {committedTargetRows}");
            Assert.True(committedTargetRows > 0, "Expected the successful run to have committed real target-year data — found none.");

            var targetYearRowExists = await TargetYearMasterRowExistsAsync();
            Assert.True(targetYearRowExists, "Expected fps.tblyearmaster to have a committed row for the target year after a fully successful run.");
        }
        finally
        {
            // A fully successful run is a real, intentional commit — there is no transaction left to
            // roll back at this point. Restore baseline the same way Phase 8's own acceptance cycle
            // will: explicit, matrix-driven cleanup.
            _output.WriteLine("Restoring baseline via matrix-driven cleanup...");
            await CleanupTargetYearAsync();
        }

        var postCleanup = await CaptureTelemetryAsync("PostCleanup");
        AssertMatchesBaseline(baseline, postCleanup, becauseSuffix: "after cleanup");
        Assert.False(await TargetYearMasterRowExistsAsync(), $"Expected fps.tblyearmaster to have no row for target year {_targetFpsYear} after cleanup.");
    }

    /// <summary>
    /// Runs the real production steps, in real DI registration order, up to and including
    /// <paramref name="stepNameToRunThrough"/> — for real, inside the real
    /// <see cref="IYearEndDataSetupTransactionManager"/> transaction — then throws a marker exception
    /// to simulate "the next step failed." Asserts the transaction manager propagates that exception
    /// and that every mutation made by the steps that ran (including ones that completed without
    /// error) is fully rolled back.
    /// </summary>
    private async Task RunInjectedFailureScenarioAsync(string scenarioLabel, string stepNameToRunThrough)
    {
        var correlationId = $"datasetup-failure-{scenarioLabel.ToLowerInvariant()}-{Guid.NewGuid():N}";
        _output.WriteLine($"=== YearEnd Data Setup rollback validation [{scenarioLabel}] | source={_sourceFpsYear} | target={_targetFpsYear} | correlationId={correlationId} ===");

        var baseline = await CaptureTelemetryAsync("Baseline");
        AssertBaselineIsClean(baseline);

        await using var provider = BuildRealServiceProvider();
        var transactionManager = provider.GetRequiredService<IYearEndDataSetupTransactionManager>();
        var steps = provider.GetServices<IYearEndDataSetupStep>().ToList();

        var cutoffIndex = steps.FindIndex(s => string.Equals(s.Name, stepNameToRunThrough, StringComparison.Ordinal));
        Assert.True(cutoffIndex >= 0, $"Step '{stepNameToRunThrough}' was not found in the resolved pipeline — has it been renamed or removed?");

        var context = new YearEndExecutionContext(correlationId, null, CurrentFpsYear: _sourceFpsYear, TargetFpsYear: _targetFpsYear);

        var thrown = await Assert.ThrowsAsync<InjectedTestFailureException>(() =>
            transactionManager.ExecuteAsync(async ct =>
            {
                for (var i = 0; i <= cutoffIndex; i++)
                {
                    var step = steps[i];
                    var startedAt = DateTime.UtcNow;
                    await step.ExecuteAsync(context, ct);
                    _output.WriteLine($"  [OK]   {step.Name} ({(DateTime.UtcNow - startedAt).TotalMilliseconds:F0}ms)");
                }

                _output.WriteLine($"  [INJECTED FAILURE] simulating a failure immediately after {steps[cutoffIndex].Name}");
                throw new InjectedTestFailureException(scenarioLabel, steps[cutoffIndex].Name);
            }, CancellationToken.None));

        _output.WriteLine($"Transaction manager correctly propagated: {thrown.Message}");

        var postRollback = await CaptureTelemetryAsync("PostRollback");
        AssertMatchesBaseline(baseline, postRollback, becauseSuffix: $"after an injected failure following {stepNameToRunThrough} ({scenarioLabel})");

        var targetYearRowExists = await TargetYearMasterRowExistsAsync();
        Assert.False(
            targetYearRowExists,
            $"Expected fps.tblyearmaster to have no row for target year {_targetFpsYear} after rollback ({scenarioLabel}) — " +
            "CreatePlannedYearStep's insert must not survive a later step's failure.");

        _output.WriteLine($"[{scenarioLabel}] Rollback verified: zero residual mutations, source year unchanged, target year row absent.");
    }

    private void AssertBaselineIsClean(TelemetrySnapshot baseline)
    {
        var baselineTargetRows = baseline.Tables.Sum(t => t.TargetYearCount);
        Assert.True(
            baselineTargetRows == 0,
            $"Expected zero target-year ({_targetFpsYear}) rows before this run, found {baselineTargetRows}. Database is not in the expected pre-run state.");
    }

    private static void AssertMatchesBaseline(TelemetrySnapshot baseline, TelemetrySnapshot after, string becauseSuffix)
    {
        var residualTargetRows = after.Tables.Sum(t => t.TargetYearCount);
        Assert.True(residualTargetRows == 0, $"Expected zero residual target-year rows {becauseSuffix}, found {residualTargetRows}.");

        var sourceCountsByTable = baseline.Tables.ToDictionary(t => (t.Schema, t.Table), t => t.SourceYearCount);
        var sourceYearUnchanged = after.Tables.All(t =>
            sourceCountsByTable.TryGetValue((t.Schema, t.Table), out var baselineSourceCount)
            && baselineSourceCount == t.SourceYearCount);

        Assert.True(sourceYearUnchanged, $"Expected source-year row counts to be unchanged {becauseSuffix}.");
    }

    /// <summary>
    /// Deletes every target-year row this pipeline could have written, driven entirely by
    /// <see cref="YearEndTableRuleMatrix"/> — the same single source of truth every production step
    /// uses — plus the target year's own <c>fps.tblyearmaster</c> row created by
    /// <c>CreatePlannedYearStep</c>. Never touches <c>mabarchive</c> (the matrix has no entries there)
    /// or the source year. Only used by the full-success scenario, which has no transaction left to
    /// roll back.
    /// </summary>
    private async Task CleanupTargetYearAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role == YearEndTableRole.GlobalReference || entry.Role == YearEndTableRole.YearScopedConfigurationDependency)
            {
                continue;
            }

            if (!await TableExistsAsync(connection, entry.Schema, entry.TableName))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, entry.Schema, entry.TableName);
            if (yearColumn is null)
            {
                continue;
            }

            await DeleteByYearAsync(connection, entry.Schema, entry.TableName, yearColumn, _targetFpsYear);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM fps.tblyearmaster WHERE fpsyear = @target_year;";
            AddParameter(command, "target_year", _targetFpsYear);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<bool> TargetYearMasterRowExistsAsync()
    {
        await using var context = CreateDbContext();
        return await context.Database
            .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblyearmaster WHERE fpsyear = {_targetFpsYear}")
            .SingleAsync() > 0;
    }

    private async Task<TelemetrySnapshot> CaptureTelemetryAsync(string phase)
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();

        var tables = new List<TableSnapshot>();

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            if (entry.Role == YearEndTableRole.GlobalReference)
            {
                continue;
            }

            if (!await TableExistsAsync(connection, entry.Schema, entry.TableName))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, entry.Schema, entry.TableName);
            if (yearColumn is null)
            {
                continue;
            }

            var sourceCount = await CountByYearAsync(connection, entry.Schema, entry.TableName, yearColumn, _sourceFpsYear);
            var targetCount = await CountByYearAsync(connection, entry.Schema, entry.TableName, yearColumn, _targetFpsYear);

            tables.Add(new TableSnapshot(entry.Schema, entry.TableName, sourceCount, targetCount));
        }

        return new TelemetrySnapshot(phase, tables);
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, string schema, string table)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1 FROM information_schema.tables
                WHERE table_schema = @schema AND table_name = @table
            );";
        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        var result = await command.ExecuteScalarAsync();
        return result is bool value && value;
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string schema, string table, string column)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = @schema AND table_name = @table AND column_name = @column
            );";
        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "column", column);
        var result = await command.ExecuteScalarAsync();
        return result is bool value && value;
    }

    private static async Task<string?> ResolveYearColumnAsync(DbConnection connection, string schema, string table)
    {
        if (await ColumnExistsAsync(connection, schema, table, "fpsyear"))
        {
            return "fpsyear";
        }

        if (await ColumnExistsAsync(connection, schema, table, "year"))
        {
            return "year";
        }

        return null;
    }

    private static async Task<long> CountByYearAsync(DbConnection connection, string schema, string table, string yearColumn, int year)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @year;";
        AddParameter(command, "year", year);
        var scalar = await command.ExecuteScalarAsync();
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    private static async Task DeleteByYearAsync(DbConnection connection, string schema, string table, string yearColumn, int year)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {schema}.{table} WHERE {yearColumn} = @year;";
        AddParameter(command, "year", year);
        await command.ExecuteNonQueryAsync();
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    /// <summary>
    /// Resolves the real 11-step pipeline via the actual production DI registration
    /// (<see cref="BatchPersistenceServiceExtensions.AddBatchPersistence"/> +
    /// <see cref="YearEndServiceExtensions.AddYearEndJob"/>) rather than a hand-copied list, so this
    /// harness cannot silently drift out of sync with the real registration if it ever changes.
    /// </summary>
    private ServiceProvider BuildRealServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:FPSConnectionString"] = _connectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddBatchPersistence(configuration);
        services.AddYearEndJob();
        return services.BuildServiceProvider();
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed class InjectedTestFailureException : Exception
    {
        public InjectedTestFailureException(string scenarioLabel, string lastSuccessfulStepName)
            : base($"Injected test failure [{scenarioLabel}] immediately after step '{lastSuccessfulStepName}' completed successfully.")
        {
        }
    }

    private sealed record TableSnapshot(string Schema, string Table, long SourceYearCount, long TargetYearCount);

    private sealed record TelemetrySnapshot(string Phase, IReadOnlyList<TableSnapshot> Tables);
}
