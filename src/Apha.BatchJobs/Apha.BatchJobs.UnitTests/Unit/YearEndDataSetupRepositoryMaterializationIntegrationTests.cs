using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for the three <see cref="YearEndDataSetupRepository"/> methods
/// added for <c>MaterializeYearEndConfigurationStep</c>, plus one test that exercises the real step class
/// against a live-resolved <c>job_queue</c> row. Row-count assertions alone (as proven by
/// <c>YearEndDataSetupRollbackValidationTests</c>) cannot catch a wrong column mapping — this proves the
/// actual staging-to-real column mapping is correct, especially <c>month_year</c> →
/// <c>fps.tlkpmonthhours.year</c>, the one deliberate rename most likely to be silently swapped with
/// <c>target_fpsyear</c>/<c>fpsyear</c> — and that the target-year mismatch guard (design decision 6)
/// fails closed against a real persisted <c>job_queue.target_fpsyear</c>, not just a mocked one. Uses
/// synthetic <c>jobqueueid</c>s and safe fake far-future years (never real business tables), so — unlike
/// the rollback-validation harness — this needs no <c>RUN_YEAR_END_ROLLBACK_VALIDATION</c> opt-in, only
/// the standard soft-skip when <c>ConnectionStrings__FPSConnectionString</c> isn't set.
/// </summary>
[Trait("Category", "Integration")]
public sealed class YearEndDataSetupRepositoryMaterializationIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";

    // Fake far-future year — avoids any collision with real fps.tblyearmaster data.
    private const int TargetFpsYear = 9091;

    private readonly string _connectionString;
    private string? _skipReason;

    public YearEndDataSetupRepositoryMaterializationIntegrationTests()
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
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ResolveJobQueueByExecutionIdAsync_WhenJobIsNotYearEndDataSetup_ReturnsNull()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        // Any other real job type — proves the fps.job_master join actually filters by jobname, not
        // just that the query compiles, so a JobExecutionId belonging to some other job (Bulk Rates,
        // RecreateSummaries, ...) never resolves as if it were a YearEnd-DataSetup request.
        await SeedJobQueueRowAsync(jobExecutionId, jobQueueId, BatchJobNames.RecreateSummary, targetFpsYear: null);

        try
        {
            var repository = CreateRepository();

            var resolved = await repository.ResolveJobQueueByExecutionIdAsync(jobExecutionId);

            Assert.Null(resolved);
        }
        finally
        {
            await CleanupJobQueueAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task MaterializeStagedConfiguration_RoundTripsEveryColumnExactly()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        await SeedJobQueueRowAsync(jobExecutionId, jobQueueId, BatchJobNames.YearEndDataSetup, TargetFpsYear);

        // fps.tblsettings/fps.tlkpmonthhours both FK to fps.tblyearmaster(fpsyear) — confirmed live,
        // not visible from the EF mapping alone. In production this is always satisfied because
        // MaterializeYearEndConfigurationStep runs after CreatePlannedYearStep; this test must seed the
        // same precondition for its own fake target year.
        await SeedTargetYearMasterRowAsync(TargetFpsYear);

        const string settingId = "ws7-materialize-test";
        const string settingValue = "distinctive-setting-value";
        const string settingNotes = "distinctive-notes";
        const short monthYear = 2031; // deliberately different from TargetFpsYear — this is the specific
                                       // value that would catch an accidental month_year/target_fpsyear swap.
        const short month = 4;
        const short fmonth = 9;
        const decimal days = 21.5m;
        const decimal cvlHours = 123.4m;
        const decimal vidHours = 67.8m;

        await using (var context = CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.yearend_settings_staging (jobqueueid, id, setting, notes)
                VALUES ({jobQueueId}, {settingId}, {settingValue}, {settingNotes});");

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.yearend_monthhours_staging (jobqueueid, month_year, month, fmonth, days, cvlhours, vidhours)
                VALUES ({jobQueueId}, {monthYear}, {month}, {fmonth}, {days}, {cvlHours}, {vidHours});");
        }

        try
        {
            var repository = CreateRepository();

            var resolved = await repository.ResolveJobQueueByExecutionIdAsync(jobExecutionId);
            Assert.NotNull(resolved);
            Assert.Equal(jobQueueId, resolved!.Value.JobQueueId);
            Assert.Equal(TargetFpsYear, resolved.Value.TargetFpsYear);

            var settingsInserted = await repository.MaterializeStagedSettingsAsync(jobQueueId, TargetFpsYear);
            Assert.Equal(1, settingsInserted);

            var monthHoursInserted = await repository.MaterializeStagedMonthHoursAsync(jobQueueId, TargetFpsYear);
            Assert.Equal(1, monthHoursInserted);

            await using var assertContext = CreateDbContext();

            var setting = await assertContext.Database
                .SqlQuery<MaterializedSetting>($@"
                    SELECT setting AS ""Setting"", notes AS ""Notes"", updated_by AS ""UpdatedBy""
                    FROM fps.tblsettings
                    WHERE fpsyear = {TargetFpsYear} AND id = {settingId}")
                .SingleAsync();

            Assert.Equal(settingValue, setting.Setting);
            Assert.Equal(settingNotes, setting.Notes);
            Assert.Equal("YearEndBatchWorker", setting.UpdatedBy);

            var monthHours = await assertContext.Database
                .SqlQuery<MaterializedMonthHours>($@"
                    SELECT year AS ""Year"", days AS ""Days"", cvlhours AS ""CvlHours"", vidhours AS ""VidHours""
                    FROM fps.tlkpmonthhours
                    WHERE fpsyear = {TargetFpsYear} AND month = {month} AND fmonth = {fmonth}")
                .SingleAsync();

            // The specific assertion that would catch month_year/target_fpsyear being accidentally
            // swapped: tlkpmonthhours.year must be the staged month_year (2031), not the target year (9091).
            Assert.Equal(monthYear, monthHours.Year);
            Assert.Equal(days, monthHours.Days);
            Assert.Equal(cvlHours, monthHours.CvlHours);
            Assert.Equal(vidHours, monthHours.VidHours);
        }
        finally
        {
            await using (var cleanupContext = CreateDbContext())
            {
                // Children before the fps.tblyearmaster parent, per the FK this test just proved is real.
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.tblsettings WHERE fpsyear = {TargetFpsYear};");
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.tlkpmonthhours WHERE fpsyear = {TargetFpsYear};");
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.tblyearmaster WHERE fpsyear = {TargetFpsYear};");
            }

            await CleanupJobQueueAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task MaterializeYearEndConfigurationStep_WhenPersistedTargetFpsYearDiffersFromContext_FailsClosedAndMaterializesNothing()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        const int persistedTargetFpsYear = 9092;
        const int contextTargetFpsYear = 9093; // deliberately different — proves the live cross-check.

        await SeedJobQueueRowAsync(jobExecutionId, jobQueueId, BatchJobNames.YearEndDataSetup, persistedTargetFpsYear);

        try
        {
            var step = new MaterializeYearEndConfigurationStep(CreateRepository(), NullLogger<MaterializeYearEndConfigurationStep>.Instance);
            var context = new YearEndExecutionContext(jobExecutionId.ToString("D"), null, CurrentFpsYear: 2025, TargetFpsYear: contextTargetFpsYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

            Assert.Contains("Target year mismatch", ex.Message, StringComparison.Ordinal);
            Assert.Contains($"target_fpsyear={persistedTargetFpsYear}", ex.Message, StringComparison.Ordinal);
            Assert.Contains($"TargetFpsYear={contextTargetFpsYear}", ex.Message, StringComparison.Ordinal);

            // Fails before any table write — no fps.tblyearmaster row was seeded for either year, so a
            // materialize attempt on either would itself fail loudly (the FK this test suite already
            // proved is real). Confirming zero rows on both is the live proof it never got that far.
            await using var assertContext = CreateDbContext();
            var persistedYearRows = await assertContext.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblsettings WHERE fpsyear = {persistedTargetFpsYear}")
                .SingleAsync();
            var contextYearRows = await assertContext.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblsettings WHERE fpsyear = {contextTargetFpsYear}")
                .SingleAsync();
            Assert.Equal(0, persistedYearRows);
            Assert.Equal(0, contextYearRows);
        }
        finally
        {
            await CleanupJobQueueAsync(jobQueueId);
        }
    }

    private async Task SeedTargetYearMasterRowAsync(int targetFpsYear)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES ({targetFpsYear}, {$"FPS{targetFpsYear}-{(targetFpsYear + 1) % 100:D2}"}, 'Planned', 'Seeded by YearEndDataSetupRepositoryMaterializationIntegrationTests.', true, 'ws7-materialize-test');");
    }

    private async Task SeedJobQueueRowAsync(Guid jobExecutionId, Guid jobQueueId, string jobName, int? targetFpsYear)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {jobName}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} LIMIT 1")
            .SingleAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, target_fpsyear)
            VALUES
                ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, 'ws7-materialize-test', NOW(), NOW(), {targetFpsYear});");
    }

    private async Task CleanupJobQueueAsync(Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.yearend_settings_staging WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.yearend_monthhours_staging WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
    }

    private IYearEndDataSetupRepository CreateRepository() => new YearEndDataSetupRepository(CreateDbContext());

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed class MaterializedSetting
    {
        public string Setting { get; init; } = null!;
        public string Notes { get; init; } = null!;
        public string UpdatedBy { get; init; } = null!;
    }

    private sealed class MaterializedMonthHours
    {
        public short Year { get; init; }
        public decimal Days { get; init; }
        public decimal CvlHours { get; init; }
        public decimal VidHours { get; init; }
    }
}
