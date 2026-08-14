using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests proving the Year End claim mechanics — canonical job
/// resolution, approval metadata validation, target-year cross-check, shared YearEnd lock, and
/// the atomic Approved -&gt; Running transition — without invoking the real Data Setup/CutOver
/// business pipeline. A fake <see cref="IBatchJobFactory"/> stands in for the real one so this
/// stays isolated to orchestration/claim behaviour (canonical handler resolution is proven
/// separately, against the real <c>BatchJobFactory</c>, in <see cref="BatchJobFactoryTests"/>).
/// </summary>
/// <remarks>
/// These tests seed/mutate <c>fps.job_queue</c> and acquire the shared <c>YearEnd</c> lock, so
/// they must only ever run against an isolated/local database — never against
/// <c>batchjob_testing</c> (the shared dev RDS instance), which is read-only-verification-only for
/// this work. Point <c>ConnectionStrings__FPSConnectionString</c> at a disposable database that
/// already has the CR048 catalogue (hyphenated job names, 7-state lifecycle) applied; otherwise
/// this class self-skips via <see cref="_yearEndHyphenatedCatalogAvailable"/>.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndOrchestratorClaimIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndHyphenatedCatalogAvailable;

    public YearEndOrchestratorClaimIntegrationTests()
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

            _yearEndHyphenatedCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname IN ({BatchJobNames.YearEndDataSetup}, {BatchJobNames.YearEndCutover})
                      AND s.status = 'Approved'")
                .SingleAsync() == 2;
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableTheory]
    [InlineData(BulkJobKind.YearEndDataSetup)]
    [InlineData(BulkJobKind.YearEndCutover)]
    public async Task RunAsync_WithPreCreatedApprovedRow_ClaimsAndLocksWithoutRunningBusinessPipeline(BulkJobKind kind)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndHyphenatedCatalogAvailable,
            "fps.job_master/job_status does not have the CR048 hyphenated Year End catalogue on this database. " +
            "This test must target an isolated/local CR048-aligned database — never batchjob_testing.");

        var jobName = kind == BulkJobKind.YearEndDataSetup ? BatchJobNames.YearEndDataSetup : BatchJobNames.YearEndCutover;
        const int plannedYear = 2099; // far-future year, safe to seed/clean without colliding with real data
        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        await SeedApprovedJobQueueRowAsync(jobName, jobExecutionId, jobQueueId, plannedYear);

        try
        {
            JobStatus? statusObservedDuringExecution = null;
            bool? lockHeldDuringExecution = null;
            var businessPipelineInvoked = false;

            var fakeJob = new CapturingFakeBatchJob(jobName, async () =>
            {
                businessPipelineInvoked = true;

                await using var probeContext = CreateDbContext();
                var probeExecRepo = new JobExecutionRepository(probeContext, NullLogger<JobExecutionRepository>.Instance);
                var execution = await probeExecRepo.GetExecutionByJobExecutionIdAsync(jobExecutionId);
                statusObservedDuringExecution = execution?.Status;

                await using var probeLockContext = CreateDbContext();
                var probeLockRepo = new BatchLockRepository(probeLockContext);
                var activeLock = await probeLockRepo.GetActiveLockAsync(BatchJobNames.YearEndLock);
                lockHeldDuringExecution = activeLock is not null && activeLock.JobQueueId == jobQueueId;
            });

            await using var orchestratorContext = CreateDbContext();
            var orchestrator = BuildOrchestrator(fakeJob, orchestratorContext);

            var parametersJson = $"{{\"plannedYear\":\"{plannedYear}\"}}";
            var result = await orchestrator.RunAsync(jobName, RunMode.Manual, jobExecutionId, "integration-test-approver", null, parametersJson);

            Assert.True(businessPipelineInvoked, "Fake job was never invoked — claim did not reach execution.");
            Assert.Equal(JobStatus.Running, statusObservedDuringExecution);
            Assert.True(lockHeldDuringExecution, "Shared YearEnd lock was not held while the job executed.");
            Assert.Equal(JobStatus.Completed, result.Status);
        }
        finally
        {
            await CleanupJobQueueRowAsync(jobQueueId);
        }
    }

    [SkippableTheory]
    [InlineData(BulkJobKind.YearEndDataSetup)]
    [InlineData(BulkJobKind.YearEndCutover)]
    public async Task RunAsync_WithConflictingPlannedYearPayload_ThrowsBeforeAnyClaim(BulkJobKind kind)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndHyphenatedCatalogAvailable,
            "fps.job_master/job_status does not have the CR048 hyphenated Year End catalogue on this database. " +
            "This test must target an isolated/local CR048-aligned database — never batchjob_testing.");

        var jobName = kind == BulkJobKind.YearEndDataSetup ? BatchJobNames.YearEndDataSetup : BatchJobNames.YearEndCutover;
        const int plannedYear = 2098; // distinct far-future year from the happy-path test
        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        await SeedApprovedJobQueueRowAsync(jobName, jobExecutionId, jobQueueId, plannedYear);

        try
        {
            var businessPipelineInvoked = false;
            var fakeJob = new CapturingFakeBatchJob(jobName, () =>
            {
                businessPipelineInvoked = true;
                return Task.CompletedTask;
            });

            await using var orchestratorContext = CreateDbContext();
            var orchestrator = BuildOrchestrator(fakeJob, orchestratorContext);

            var conflictingParametersJson = $"{{\"plannedYear\":\"{plannedYear}\",\"targetFpsYear\":{plannedYear + 1}}}";

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                orchestrator.RunAsync(jobName, RunMode.Manual, jobExecutionId, "integration-test-approver", null, conflictingParametersJson));

            Assert.False(businessPipelineInvoked, "Fake job must not run when the payload's year fields conflict.");

            await using var context = CreateDbContext();
            var execRepo = new JobExecutionRepository(context, NullLogger<JobExecutionRepository>.Instance);
            var execution = await execRepo.GetExecutionByJobExecutionIdAsync(jobExecutionId);
            Assert.Equal(JobStatus.Approved, execution?.Status);

            var lockRepo = new BatchLockRepository(context);
            var activeLock = await lockRepo.GetActiveLockAsync(BatchJobNames.YearEndLock);
            Assert.Null(activeLock);
        }
        finally
        {
            await CleanupJobQueueRowAsync(jobQueueId);
        }
    }

    public enum BulkJobKind
    {
        YearEndDataSetup,
        YearEndCutover
    }

    private static JobOrchestrator BuildOrchestrator(IBatchJob fakeJob, BatchJobsDbContext context)
    {
        var factory = new FakeBatchJobFactory(fakeJob);
        var lockRepository = new BatchLockRepository(context);
        var executionRepository = new JobExecutionRepository(context, NullLogger<JobExecutionRepository>.Instance);
        var correlationService = Substitute.For<ICorrelationService>();
        var currentExecutionContext = Substitute.For<ICurrentJobExecutionContext>();
        var notificationService = Substitute.For<IEmailNotificationService>();
        var alertingSettings = Options.Create(new BatchAlertingSettings { EnableEmailNotifications = false, EmailEnabledJobs = [] });
        var settings = Options.Create(new BatchJobSettings { JobTimeout = 3600 });
        var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

        return new JobOrchestrator(
            factory,
            lockRepository,
            executionRepository,
            correlationService,
            currentExecutionContext,
            notificationService,
            alertingSettings,
            settings,
            configuration,
            NullLogger<JobOrchestrator>.Instance);
    }

    private async Task SeedApprovedJobQueueRowAsync(string jobName, Guid jobExecutionId, Guid jobQueueId, int plannedYear)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {jobName}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
            .SingleAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime,
                 fpsyear, approved_by, approved_at_utc)
            VALUES
                ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, 'integration-test-requester', NOW(), NOW(),
                 {plannedYear}, 'integration-test-approver', NOW());");
    }

    private async Task CleanupJobQueueRowAsync(Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_lock WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private sealed class CapturingFakeBatchJob : IBatchJob
    {
        private readonly Func<Task> _onExecute;

        public CapturingFakeBatchJob(string name, Func<Task> onExecute)
        {
            Name = name;
            _onExecute = onExecute;
        }

        public string Name { get; }
        public string IdempotencyStrategy => "ApprovedRowClaimWithYearEndLock";
        public string? ScheduleExpression => null;
        public string? ScheduleDescription => "Fake job for orchestrator claim proof";
        public int? MaxExecutionSeconds => null;

        public Task ExecuteAsync(CancellationToken cancellationToken = default) => _onExecute();
    }

    private sealed class FakeBatchJobFactory : IBatchJobFactory
    {
        private readonly IBatchJob _job;

        public FakeBatchJobFactory(IBatchJob job) => _job = job;

        public IBatchJob Create(string jobName) => _job;
        public IEnumerable<string> GetAvailableJobs() => new[] { _job.Name };
    }
}
