using System.Data.Common;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests proving <see cref="YearEndDataSetupService"/> owns the
/// single connection/transaction shared by every step, commits only after the final step
/// succeeds, and rolls back the entire pipeline (leaving no partial business-data commit) on any
/// failure.
/// </summary>
/// <remarks>
/// Must only ever run against an isolated/local database — never <c>batchjob_testing</c> (the
/// shared dev RDS instance, read-only-verification-only for this work). Deferred until an
/// isolated CR048-aligned database is available; self-skips otherwise via
/// <see cref="ConnectionStrings__FPSConnectionString"/> defaulting to an unreachable local
/// connection.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndDataSetupServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _exactlyOneOpenYear;
    private int _currentOpenYear;

    public YearEndDataSetupServiceIntegrationTests()
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

            var openYears = await context.Database
                .SqlQuery<int>($@"SELECT fpsyear AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus = 'Open'")
                .ToListAsync();

            _exactlyOneOpenYear = openYears.Count == 1;
            if (_exactlyOneOpenYear)
            {
                _currentOpenYear = openYears[0];
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_WithFakeSteps_ExecutesInOrderAndThreadsContextForward()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var invocationOrder = new List<string>();
        var stepA = new RecordingStep("StepA", invocationOrder, ctx => ctx with { CurrentFpsYear = 4242 });
        var stepB = new RecordingStep("StepB", invocationOrder, ctx => ctx);

        var dbContextFactory = CreateDbContextFactory();
        var service = new YearEndDataSetupService(dbContextFactory, [stepA, stepB], NullLogger<YearEndDataSetupService>.Instance);
        var context = new YearEndExecutionContext("corr-order", null, CurrentFpsYear: null, TargetFpsYear: 4243);

        await service.ExecuteAsync(context);

        Assert.Equal(["StepA", "StepB"], invocationOrder);
        // StepB must have observed StepA's context update — proves the returned context threads forward.
        Assert.Equal(4242, stepB.ObservedCurrentFpsYear);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenLateStepFails_RollsBackAndLeavesNoPartialCommit()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _exactlyOneOpenYear,
            "fps.tblyearmaster does not have exactly one Open year on this database. " +
            "This test must target an isolated/local CR048-aligned database — never batchjob_testing.");

        var targetYear = _currentOpenYear + 500; // far-future, safe to seed/clean without colliding with real data
        var dbContextFactory = CreateDbContextFactory();

        try
        {
            var service = new YearEndDataSetupService(
                dbContextFactory,
                [
                    new ValidateYearEndContextStep(),
                    new CreatePlannedYearStep(NullLogger<CreatePlannedYearStep>.Instance),
                    new AlwaysFailingStep()
                ],
                NullLogger<YearEndDataSetupService>.Instance);

            var context = new YearEndExecutionContext("corr-rollback", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

            // Fresh connection, separate from anything the service used — proves the rollback
            // actually committed at the database level, not just in the failed in-process call.
            await using var verifyContext = CreateDbContext();
            var yearMasterRowCount = await verifyContext.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblyearmaster WHERE fpsyear = {targetYear}")
                .SingleAsync();

            Assert.Equal(0, yearMasterRowCount);
        }
        finally
        {
            await using var cleanupContext = CreateDbContext();
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.tblyearmaster WHERE fpsyear = {targetYear};");
        }
    }

    private IDbContextFactory<BatchJobsDbContext> CreateDbContextFactory() => new TestDbContextFactory(_connectionString);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed class TestDbContextFactory : IDbContextFactory<BatchJobsDbContext>
    {
        private readonly string _connectionString;

        public TestDbContextFactory(string connectionString) => _connectionString = connectionString;

        public BatchJobsDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return new BatchJobsDbContext(options);
        }
    }

    private sealed class RecordingStep : IYearEndDataSetupStep
    {
        private readonly List<string> _invocationOrder;
        private readonly Func<YearEndExecutionContext, YearEndExecutionContext> _transform;

        public RecordingStep(string name, List<string> invocationOrder, Func<YearEndExecutionContext, YearEndExecutionContext> transform)
        {
            Name = name;
            _invocationOrder = invocationOrder;
            _transform = transform;
        }

        public string Name { get; }
        public int? ObservedCurrentFpsYear { get; private set; }

        public Task<YearEndExecutionContext> ExecuteAsync(
            YearEndExecutionContext context,
            DbConnection connection,
            DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            _invocationOrder.Add(Name);
            ObservedCurrentFpsYear = context.CurrentFpsYear;
            return Task.FromResult(_transform(context));
        }
    }

    private sealed class AlwaysFailingStep : IYearEndDataSetupStep
    {
        public string Name => "AlwaysFailingStep";

        public Task<YearEndExecutionContext> ExecuteAsync(
            YearEndExecutionContext context,
            DbConnection connection,
            DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Deliberate failure to prove the shared transaction rolls back.");
        }
    }
}
