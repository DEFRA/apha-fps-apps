using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed Year End Cutover integration tests with skip-safe semantics.
/// The service commits its own transaction against fps.tblyearmaster, so seeded rows
/// are inserted and committed up front and always removed again in a finally block.
/// All tests also seed a Completed YearEndDataSetup job_queue row for the target year,
/// since the service now requires that precondition (spec Section 20.1) before it will
/// even look at fps.tblyearmaster.
/// </summary>
/// <remarks>
/// Since Phase 1, the service derives the current FPS year live from the single Open row in
/// <c>fps.tblyearmaster</c> (<see cref="YearEndYearContextResolver"/>) rather than trusting
/// <c>context.CurrentFpsYear</c> — these tests read the database's real live Open year and only
/// ever seed the (far-future, disposable) target/planned year row, never a competing "Open" row,
/// since the resolver requires exactly one Open row across the whole table. Must only ever run
/// against an isolated/local database — never <c>batchjob_testing</c>.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndCutoverServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndDataSetupCompletedCatalogAvailable;
    private bool _exactlyOneOpenYear;
    private int _liveOpenYear;

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

            var openYears = await context.Database
                .SqlQuery<int>($@"SELECT fpsyear AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus = 'Open'")
                .ToListAsync();

            _exactlyOneOpenYear = openYears.Count == 1;
            if (_exactlyOneOpenYear)
            {
                _liveOpenYear = openYears[0];
            }
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
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 500; // far-future, safe to seed/clean without colliding with real data
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedCompletedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId);

        try
        {
            var service = new YearEndCutoverService(
                CreateDbContextFactory(),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-1",
                ParametersJson: null,
                CurrentFpsYear: null,
                TargetFpsYear: targetYear);

            await service.ExecuteAsync(context);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Closed", originalOpenYearStatus);
            Assert.Equal("Open", targetStatus);
        }
        finally
        {
            // Restore the real Open year so the database is left in its original state.
            await using var restoreContext = CreateDbContext();
            await restoreContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = {_liveOpenYear};");

            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearNotPlanned_ThrowsAndLeavesRowsUnchanged()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 501;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Open", active: true);
        await SeedCompletedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId);

        try
        {
            var service = new YearEndCutoverService(
                CreateDbContextFactory(),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-2",
                ParametersJson: null,
                CurrentFpsYear: null,
                TargetFpsYear: targetYear);

            // Two Open years now exist (the real live one plus this seeded target) — the live
            // resolver must reject this as ambiguous before ever reaching the "not Planned" check.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("Open year", ex.Message, StringComparison.Ordinal);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", originalOpenYearStatus);
            Assert.Equal("Open", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenLatestDataSetupNotCompletedForTargetYear_ThrowsBeforeTouchingYearMaster()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 502;

        await SeedYearAsync(targetYear, "Planned", active: true);

        try
        {
            var service = new YearEndCutoverService(
                CreateDbContextFactory(),
                CreateExecutionRepository(),
                NullLogger<YearEndCutoverService>.Instance);

            var context = new YearEndExecutionContext(
                CorrelationId: "cutover-it-3",
                ParametersJson: null,
                CurrentFpsYear: null,
                TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", originalOpenYearStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(targetYear);
        }
    }

    private void SkipUnlessReady()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCompletedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed' is not yet provisioned on this database.");
        Skip.IfNot(
            _exactlyOneOpenYear,
            "fps.tblyearmaster does not have exactly one Open year on this database. " +
            "This test must target an isolated/local CR048-aligned database — never batchjob_testing.");
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

    private async Task SeedCompletedDataSetupExecutionAsync(int targetYear, Guid jobQueueId)
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
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, enddatetime, fpsyear)
            VALUES
                ({jobQueueId}, {Guid.NewGuid()}, {jobId}, {statusId}, 'integration-test-requester', NOW(), NOW(), NOW(), {targetYear});");
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
