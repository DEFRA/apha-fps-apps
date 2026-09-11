using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed Year End Cutover integration tests with skip-safe semantics.
/// The service commits its own transaction against fps.tblyearmaster, so seeded rows
/// are inserted and committed up front and always removed again in a finally block.
/// Both tests also seed a Completed YearEndDataSetup job_queue row for the target year,
/// since the service now requires that precondition (spec Section 20.1) before it will
/// even look at fps.tblyearmaster.
/// </summary>
[Trait("Category", "Integration")]
public sealed class YearEndCutoverServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndDataSetupCompletedCatalogAvailable;

    public YearEndCutoverServiceIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            _yearEndDataSetupCompletedCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.YearEndDataSetup}
                      AND s.status = 'Completed'")
                .SingleAsync() > 0;
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_WhenPreconditionsMet_ClosesCurrentYearAndActivatesTargetYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9801;
        const int targetYear = 9802;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedCompletedDataSetupExecutionAsync(currentYear, targetYear, dataSetupJobQueueId);

        try
        {
            var service = new YearEndCutoverService(
                new YearEndCutoverRepository(CreateDbContextFactory()),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-1",
                ParametersJson: null,
                CurrentFpsYear: currentYear,
                TargetFpsYear: targetYear);

            await service.ExecuteAsync(context);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Closed", currentStatus);
            Assert.Equal("Open", targetStatus);

            // Phase 6 hardening: staging tables must be empty after a successful cutover,
            // regardless of what (if anything) PACT import activity left in them beforehand — this
            // assertion is valid whether or not the tables had rows before this test ran.
            foreach (var table in StagingTables)
            {
                var remaining = await CountRowsAsync(table);
                Assert.Equal(0, remaining);
            }
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteCutoverAsync_WhenLatestDataSetupExecutionForTargetYearIsNotCompleted_ThrowsEvenWhenAnOlderCompletedRowExists()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9807;
        const int targetYear = 9808;

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);

        // Older row: Completed. Newer row: Failed. Proves the repository's in-transaction
        // predecessor check uses the LATEST matching execution (ORDER BY startdatetime DESC),
        // not just "does any Completed row exist for this target year" — the older Completed row
        // alone would wrongly let this through if the query weren't order-sensitive.
        var olderCompletedJobQueueId = Guid.NewGuid();
        var newerFailedJobQueueId = Guid.NewGuid();
        await SeedDataSetupExecutionAsync(currentYear, targetYear, olderCompletedJobQueueId, "Completed", DateTime.UtcNow.AddHours(-1));
        await SeedDataSetupExecutionAsync(currentYear, targetYear, newerFailedJobQueueId, "Failed", DateTime.UtcNow);

        try
        {
            var repository = new YearEndCutoverRepository(CreateDbContextFactory());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.ExecuteCutoverAsync(currentYear, targetYear));

            Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);
            Assert.Contains("'Failed'", ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", currentStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(olderCompletedJobQueueId);
            await DeleteJobQueueRowAsync(newerFailedJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteCutoverAsync_WhenPredecessorRowHasNullTargetFpsYear_TreatsItAsNotFoundAndThrows()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9814;
        const int targetYear = 9815;
        var jobQueueId = Guid.NewGuid();

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);
        // Deliberately fpsyear = targetYear (the OLD defect-masking shape a legacy/pre-migration row
        // could have) but target_fpsyear left NULL — proves the fix doesn't fall back to matching on
        // fpsyear when target_fpsyear is absent, which would quietly reintroduce Defect C.
        await SeedDataSetupExecutionWithNullTargetFpsYearAsync(targetYear, jobQueueId, "Completed", DateTime.UtcNow);

        try
        {
            var repository = new YearEndCutoverRepository(CreateDbContextFactory());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.ExecuteCutoverAsync(currentYear, targetYear));

            Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);
            Assert.Contains("'None'", ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", currentStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task GetLastExecutionByTargetFpsYearAsync_FindsCompletedPredecessorByTargetFpsYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        // Tests the Defect C lookup contract directly against fps.job_queue/job_master/job_status only —
        // no fps.tblyearmaster row, no ExecuteCutoverAsync transaction, no year-status flip. Deliberately
        // isolated from the global "exactly one Open year" invariant (see
        // fps-year-end-cutover-validate-final-year-state-global-open-year-finding-2026-09-03.md) — this
        // proves the lookup itself, without touching year state at all.
        const int currentYear = 9816;
        const int targetYear = 9817;
        var jobQueueId = Guid.NewGuid();

        await SeedCompletedDataSetupExecutionAsync(currentYear, targetYear, jobQueueId);

        try
        {
            var repository = CreateExecutionRepository();

            var record = await repository.GetLastExecutionByTargetFpsYearAsync(BatchJobNames.YearEndDataSetup, targetYear);

            Assert.NotNull(record);
            Assert.Equal(JobStatus.Completed, record!.Status);
        }
        finally
        {
            await DeleteJobQueueRowAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task GetLastExecutionByTargetFpsYearAsync_WhenPredecessorHasNullTargetFpsYear_ReturnsNull()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int targetYear = 9818;
        var jobQueueId = Guid.NewGuid();

        // Legacy shape: fpsyear = targetYear (would have matched the OLD defective lookup), target_fpsyear
        // left NULL. No fps.tblyearmaster row needed — this method never reads that table.
        await SeedDataSetupExecutionWithNullTargetFpsYearAsync(targetYear, jobQueueId, "Completed", DateTime.UtcNow);

        try
        {
            var repository = CreateExecutionRepository();

            var record = await repository.GetLastExecutionByTargetFpsYearAsync(BatchJobNames.YearEndDataSetup, targetYear);

            Assert.Null(record);
        }
        finally
        {
            await DeleteJobQueueRowAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteCutoverAsync_WhenStagingTableIsLocked_ThrowsAndLeavesYearRowsUnchanged()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9809;
        const int targetYear = 9810;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedCompletedDataSetupExecutionAsync(currentYear, targetYear, dataSetupJobQueueId);

        // Hold an exclusive lock on one staging table from a separate connection/transaction that
        // stays open for the duration of the cutover attempt below — simulates in-flight PACT
        // import activity. Never committed/rolled back until this test's own finally block.
        await using var lockingContext = CreateDbContext();
        await lockingContext.Database.OpenConnectionAsync();
        await using var lockingTransaction = await lockingContext.Database.BeginTransactionAsync();
        await lockingContext.Database.ExecuteSqlRawAsync($"LOCK TABLE {StagingTables[0]} IN ACCESS EXCLUSIVE MODE;");

        try
        {
            var repository = new YearEndCutoverRepository(CreateDbContextFactory());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.ExecuteCutoverAsync(currentYear, targetYear));

            Assert.Contains(StagingTables[0], ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", currentStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await lockingTransaction.RollbackAsync();
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteCutoverAsync_WhenAnotherYearIsUnexpectedlyOpen_ThrowsAtFinalValidationAndRollsBackStatusFlips()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9811;
        const int targetYear = 9812;
        // An unrelated third year, already Open, present for the entire test — its existence is
        // exactly what makes ValidateFinalYearStateAsync's "exactly one Open year" check fail once
        // the status flips (current->Closed, target->Open) have already happened inside the same
        // transaction, proving the whole transaction — flips included — rolls back on a
        // final-assertion failure, not just on an up-front precondition failure.
        const int unrelatedOpenYear = 9813;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedYearAsync(unrelatedOpenYear, "Open", active: true);
        await SeedCompletedDataSetupExecutionAsync(currentYear, targetYear, dataSetupJobQueueId);

        try
        {
            var repository = new YearEndCutoverRepository(CreateDbContextFactory());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.ExecuteCutoverAsync(currentYear, targetYear));

            Assert.Contains("exactly one Open year", ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);
            var (unrelatedStatus, _) = await GetYearStateAsync(unrelatedOpenYear);

            // Proves rollback undid the flips even though they were already applied and committed
            // to the transaction's own view of the data before the final check ran.
            Assert.Equal("Open", currentStatus);
            Assert.Equal("Planned", targetStatus);
            Assert.Equal("Open", unrelatedStatus);
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteYearAsync(unrelatedOpenYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearNotPlanned_ThrowsAndLeavesRowsUnchanged()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9803;
        const int targetYear = 9804;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Open", active: true);
        await SeedCompletedDataSetupExecutionAsync(currentYear, targetYear, dataSetupJobQueueId);

        try
        {
            var service = new YearEndCutoverService(
                new YearEndCutoverRepository(CreateDbContextFactory()),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-2",
                ParametersJson: null,
                CurrentFpsYear: currentYear,
                TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("Planned", ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", currentStatus);
            Assert.Equal("Open", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenLatestDataSetupNotCompletedForTargetYear_ThrowsBeforeTouchingYearMaster()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");

        const int currentYear = 9805;
        const int targetYear = 9806;

        await SeedYearAsync(currentYear, "Open", active: true);
        await SeedYearAsync(targetYear, "Planned", active: true);

        try
        {
            var service = new YearEndCutoverService(
                new YearEndCutoverRepository(CreateDbContextFactory()),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-3",
                ParametersJson: null,
                CurrentFpsYear: currentYear,
                TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);

            var (currentStatus, _) = await GetYearStateAsync(currentYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", currentStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(currentYear);
            await DeleteYearAsync(targetYear);
        }
    }

    /// <summary>The three PACT-owned staging tables Phase 6 hardening locks/truncates during cutover.</summary>
    private static readonly string[] StagingTables =
    {
        "fps.proj_subcontract_staging",
        "fps.tblstagingmonthlyoutput",
        "fps.tblstagingmonthlytime"
    };

    private async Task<long> CountRowsAsync(string qualifiedTableName)
    {
        await using var context = CreateDbContext();
        return await context.Database
            .SqlQueryRaw<long>($@"SELECT COUNT(*)::bigint AS ""Value"" FROM {qualifiedTableName}")
            .SingleAsync();
    }

    /// <summary>
    /// Generalizes <see cref="SeedCompletedDataSetupExecutionAsync"/> to an arbitrary status and
    /// start time, so a test can control which of two job_queue rows for the same target year is
    /// "latest" by <c>startdatetime</c>. <paramref name="currentYear"/> and <paramref name="targetYear"/>
    /// are deliberately independent, explicit parameters, not derived from each other — matches real
    /// producer shape (<c>fpsyear</c> = the request's own current/open year, <c>target_fpsyear</c> = the
    /// year it's preparing) rather than baking in a "target = current + 1" business assumption that has
    /// nothing to do with the predecessor lookup being tested.
    /// </summary>
    private async Task SeedDataSetupExecutionAsync(int currentYear, int targetYear, Guid jobQueueId, string status, DateTime startDateTimeUtc)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"
                SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"
                SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = {status}")
            .SingleAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear, target_fpsyear)
            VALUES
                ({jobQueueId}, {Guid.NewGuid()}, {jobId}, {statusId}, 'integration-test-requester', NOW(), {startDateTimeUtc}, {currentYear}, {targetYear});");
    }

    /// <summary>
    /// Seeds a Data Setup <c>job_queue</c> row with <c>target_fpsyear</c> deliberately left
    /// <c>NULL</c> — the legacy/pre-migration shape. Proves the predecessor lookup fails closed (treats
    /// it as "no predecessor") rather than falling back to matching on <c>fpsyear</c>.
    /// </summary>
    private async Task SeedDataSetupExecutionWithNullTargetFpsYearAsync(int fpsYear, Guid jobQueueId, string status, DateTime startDateTimeUtc)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"
                SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"
                SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = {status}")
            .SingleAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear)
            VALUES
                ({jobQueueId}, {Guid.NewGuid()}, {jobId}, {statusId}, 'integration-test-requester', NOW(), {startDateTimeUtc}, {fpsYear});");
    }

    private async Task SeedYearAsync(int fpsYear, string yearStatus, bool active)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.tblyearmaster WHERE fpsyear = {fpsYear};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES ({fpsYear}, {$"IT{fpsYear}"}, {yearStatus}, 'YearEndCutoverServiceIntegrationTests', {active}, 'IntegrationTest');");
    }

    /// <summary>
    /// <paramref name="currentYear"/> and <paramref name="targetYear"/> are deliberately independent,
    /// explicit parameters — see <see cref="SeedDataSetupExecutionAsync"/>'s doc comment for why.
    /// </summary>
    private async Task SeedCompletedDataSetupExecutionAsync(int currentYear, int targetYear, Guid jobQueueId)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"
                SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"
                SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Completed'")
            .SingleAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, enddatetime, fpsyear, target_fpsyear)
            VALUES
                ({jobQueueId}, {Guid.NewGuid()}, {jobId}, {statusId}, 'integration-test-requester', NOW(), NOW(), NOW(), {currentYear}, {targetYear});");
    }

    private async Task DeleteJobQueueRowAsync(Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
    }

    private async Task<(string YearStatus, bool Active)> GetYearStateAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        var row = await context.Database
            .SqlQuery<YearStateRow>($@"
                SELECT ym.yearstatus AS ""YearStatus"", ym.active AS ""Active""
                FROM fps.tblyearmaster ym
                WHERE ym.fpsyear = {fpsYear}")
            .SingleAsync();

        return (row.YearStatus, row.Active);
    }

    private async Task DeleteYearAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.tblyearmaster WHERE fpsyear = {fpsYear};");
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private IDbContextFactory<BatchJobsDbContext> CreateDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new TestDbContextFactory(options);
    }

    private JobExecutionRepository CreateExecutionRepository() =>
        new(CreateDbContext(), NullLogger<JobExecutionRepository>.Instance);

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed class YearStateRow
    {
        public string YearStatus { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    private sealed class TestDbContextFactory : IDbContextFactory<BatchJobsDbContext>
    {
        private readonly DbContextOptions<BatchJobsDbContext> _options;

        public TestDbContextFactory(DbContextOptions<BatchJobsDbContext> options)
        {
            _options = options;
        }

        public BatchJobsDbContext CreateDbContext() => new(_options);
    }
}
