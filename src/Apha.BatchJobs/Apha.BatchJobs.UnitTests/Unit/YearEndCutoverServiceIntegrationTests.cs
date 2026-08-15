using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed Year End Cutover integration tests with skip-safe semantics, covering the
/// frozen 2026-08-15 CutOver design end to end: current Open -&gt; Closed, target Planned -&gt; Open,
/// Phase 4 staging cleanup (three PACT-owned tables, "Option A+" exclusive-lock precondition,
/// <c>TRUNCATE ... RESTART IDENTITY</c>), the mandatory post-update assertion, and every safeguard
/// named in <c>fps-year-end-cutover-contract-trace-and-open-questions-2026-08-15.md</c>. The service
/// commits its own transaction (year rows + staging tables), so seeded rows are inserted and
/// committed up front and always removed again in a <c>finally</c> block.
/// </summary>
/// <remarks>
/// Since Phase 1, the service derives the current FPS year live from the single Open row in
/// <c>fps.tblyearmaster</c> (<see cref="YearEndYearContextResolver"/>) rather than trusting
/// <c>context.CurrentFpsYear</c> — these tests read the database's real live Open year and only
/// ever seed the (far-future, disposable) target/planned year row, except the zero/multiple-Open
/// tests, which deliberately and temporarily mutate the live Open year's own status (restored in
/// <c>finally</c>, same pattern the happy-path test already uses for the opposite direction). Must
/// only ever run against an isolated/local database — never <c>batchjob_testing</c>.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class YearEndCutoverServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndDataSetupCatalogAvailable;
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

            _yearEndDataSetupCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.YearEndDataSetup}
                      AND s.status IN ('Completed', 'Failed')")
                .SingleAsync() >= 2;

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

    // ─── Happy path (state transition + Phase 4 staging cleanup) ──────────────────────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenPreconditionsMet_ClosesCurrentYearActivatesTargetYearAndClearsStaging()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 500; // far-future, safe to seed/clean without colliding with real data
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId, "Completed");
        await SeedStagingRowsAsync();

        try
        {
            var service = CreateService();
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

            var (subcontractCount, monthlyOutputCount, monthlyTimeCount) = await GetStagingRowCountsAsync();
            Assert.Equal(0, subcontractCount);
            Assert.Equal(0, monthlyOutputCount);
            Assert.Equal(0, monthlyTimeCount);

            // Prove RESTART IDENTITY genuinely reset the sequence, not merely "table is empty".
            var newSubcontractId = await InsertProbeSubcontractRowAsync();
            var newOutputId = await InsertProbeMonthlyOutputRowAsync();
            var newTimeId = await InsertProbeMonthlyTimeRowAsync();

            Assert.Equal(1, newSubcontractId);
            Assert.Equal(1, newOutputId);
            Assert.Equal(1, newTimeId);
        }
        finally
        {
            // Restore the real Open year so the database is left in its original state.
            await using var restoreContext = CreateDbContext();
            await restoreContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = {_liveOpenYear};");

            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
            await TruncateStagingTablesAsync();
        }
    }

    // ─── "CutOver executed twice" — Test B: a second, independently-approved execution ────

    [SkippableFact]
    public async Task ExecuteAsync_WhenRunTwiceForSameTarget_SecondRunFailsOnBusinessStateNotQueue()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 503;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId, "Completed");

        try
        {
            var service = CreateService();

            var firstContext = new YearEndExecutionContext("cutover-it-twice-1", null, CurrentFpsYear: null, TargetFpsYear: targetYear);
            await service.ExecuteAsync(firstContext);

            var (afterFirstOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (afterFirstTargetStatus, _) = await GetYearStateAsync(targetYear);
            Assert.Equal("Closed", afterFirstOpenYearStatus);
            Assert.Equal("Open", afterFirstTargetStatus);

            // A fresh, independently-approved execution for the same target — the queue/claim
            // mechanism is not what stops this (different jobExecutionId, nothing replayed);
            // business state must catch it. Concretely: targetYear IS NOW the current live Open
            // year (the first run just made it so), so the service's own "plannedYear must be
            // greater than the current Open year" guard fires before it ever reaches the
            // target-must-be-Planned check — a different, but equally valid, business-state
            // rejection of "cut over to the year that's already current."
            var secondContext = new YearEndExecutionContext("cutover-it-twice-2", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(secondContext));
            Assert.Contains("greater than the current Open year", ex.Message, StringComparison.Ordinal);

            // No further transition performed — state identical to right after the first run.
            var (afterSecondOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (afterSecondTargetStatus, _) = await GetYearStateAsync(targetYear);
            Assert.Equal("Closed", afterSecondOpenYearStatus);
            Assert.Equal("Open", afterSecondTargetStatus);
        }
        finally
        {
            await using var restoreContext = CreateDbContext();
            await restoreContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = {_liveOpenYear};");

            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    // ─── Target isn't Planned (fixed — no longer conflated with multiple-Open-years) ──────

    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearIsClosed_ThrowsTargetNotPlanned()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 504;

        // Deliberately Closed, not Open — exactly one Open year exists throughout (the live one),
        // so this exercises the real "target must be Planned" branch rather than the
        // ambiguous-multiple-Open-years branch the old version of this test accidentally hit.
        await SeedYearAsync(targetYear, "Closed", active: true);

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-2", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("Planned", ex.Message, StringComparison.Ordinal);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", originalOpenYearStatus);
            Assert.Equal("Closed", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(targetYear);
        }
    }

    // ─── Current isn't Open — zero and multiple Open years, kept as separate tests ────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenNoOpenYearExists_ThrowsBeforeTouchingYearMaster()
    {
        SkipUnlessReady();

        await using (var context = CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Closed' WHERE fpsyear = {_liveOpenYear};");
        }

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-3a", null, CurrentFpsYear: null, TargetFpsYear: _liveOpenYear + 505);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("No Open year", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            await using var restoreContext = CreateDbContext();
            await restoreContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = {_liveOpenYear};");
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenMultipleOpenYearsExist_ThrowsBeforeTouchingYearMaster()
    {
        SkipUnlessReady();

        var secondOpenYear = _liveOpenYear + 506;
        await SeedYearAsync(secondOpenYear, "Open", active: true);

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-3b", null, CurrentFpsYear: null, TargetFpsYear: secondOpenYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("Multiple Open years", ex.Message, StringComparison.Ordinal);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            Assert.Equal("Open", originalOpenYearStatus);
        }
        finally
        {
            await DeleteYearAsync(secondOpenYear);
        }
    }

    // ─── Data Setup precondition — both "no execution" and "found but not Completed" ──────

    [SkippableFact]
    public async Task ExecuteAsync_WhenLatestDataSetupNotCompletedForTargetYear_ThrowsBeforeTouchingYearMaster()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 502;

        await SeedYearAsync(targetYear, "Planned", active: true);

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-3", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);
            Assert.Contains("None", ex.Message, StringComparison.Ordinal);

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

    [SkippableFact]
    public async Task ExecuteAsync_WhenLatestDataSetupExecutionFoundButNotCompleted_ThrowsBeforeTouchingYearMaster()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 507;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId, "Failed");

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-3c", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("Failed", ex.Message, StringComparison.Ordinal);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);

            Assert.Equal("Open", originalOpenYearStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    // ─── Phase 4 staging-in-use precondition ("Option A+" exclusive lock, NOWAIT) ─────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenStagingTableIsLockedByAnotherProcess_ThrowsAndLeavesEverythingUnchanged()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 508;
        var dataSetupJobQueueId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId, "Completed");

        await using var contendingContext = CreateDbContext();
        await using var contendingTransaction = await contendingContext.Database.BeginTransactionAsync();
        await contendingContext.Database.ExecuteSqlRawAsync("LOCK TABLE fps.tblstagingmonthlytime IN ACCESS EXCLUSIVE MODE;");

        try
        {
            var service = CreateService();
            var context = new YearEndExecutionContext("cutover-it-lock", null, CurrentFpsYear: null, TargetFpsYear: targetYear);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));
            Assert.Contains("tblstagingmonthlytime", ex.Message, StringComparison.Ordinal);
            Assert.Contains("in use", ex.Message, StringComparison.OrdinalIgnoreCase);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);
            Assert.Equal("Open", originalOpenYearStatus);
            Assert.Equal("Planned", targetStatus);
        }
        finally
        {
            await contendingTransaction.RollbackAsync();
            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
        }
    }

    // ─── Rollback atomicity — the most important safeguard ────────────────────────────────

    /// <summary>
    /// Proves the year switch AND Phase 4 staging cleanup roll back together as one atomic unit.
    /// Does not call <see cref="YearEndCutoverService.ExecuteAsync"/> directly — that method owns
    /// and commits its own transaction on full success, so there is no seam to force a mid-flight
    /// failure through the public API. Instead this test opens its own transaction, replicates the
    /// same sequence of SQL operations the service performs (resolve/lock years, flip statuses,
    /// truncate staging), captures in-transaction state, then rolls back instead of committing —
    /// the identical pattern already used by <c>YearEndDataSetupRollbackValidationTests</c> for the
    /// same underlying reason. A deliberate <c>ROLLBACK</c> proves exactly the same Postgres
    /// atomicity guarantee an exception-triggered one would — the database doesn't distinguish
    /// between the two — without needing a throw/catch purely for test plumbing.
    /// </summary>
    [SkippableFact]
    public async Task Cutover_WhenFailureOccursAfterMutationBeforeCommit_RollsBackYearSwitchAndStagingTogether()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 509;
        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedStagingRowsAsync();

        var (baselineSubcontractCount, baselineOutputCount, baselineTimeCount) = await GetStagingRowCountsAsync();
        Assert.True(baselineSubcontractCount > 0 && baselineOutputCount > 0 && baselineTimeCount > 0, "Staging seed did not take effect.");

        try
        {
            await using var context = CreateDbContext();
            await context.Database.OpenConnectionAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var connection = context.Database.GetDbConnection();
            var dbTransaction = transaction.GetDbTransaction();

            await using (var lockCmd = connection.CreateCommand())
            {
                lockCmd.Transaction = dbTransaction;
                lockCmd.CommandText =
                    "LOCK TABLE fps.proj_subcontract_staging, fps.tblstagingmonthlyoutput, fps.tblstagingmonthlytime IN ACCESS EXCLUSIVE MODE NOWAIT;";
                await lockCmd.ExecuteNonQueryAsync();
            }

            await using (var updateCurrentCmd = connection.CreateCommand())
            {
                updateCurrentCmd.Transaction = dbTransaction;
                updateCurrentCmd.CommandText = "UPDATE fps.tblyearmaster SET yearstatus = 'Closed' WHERE fpsyear = @fpsyear;";
                AddParam(updateCurrentCmd, "fpsyear", _liveOpenYear);
                await updateCurrentCmd.ExecuteNonQueryAsync();
            }

            await using (var updateTargetCmd = connection.CreateCommand())
            {
                updateTargetCmd.Transaction = dbTransaction;
                updateTargetCmd.CommandText = "UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = @fpsyear;";
                AddParam(updateTargetCmd, "fpsyear", targetYear);
                await updateTargetCmd.ExecuteNonQueryAsync();
            }

            await using (var truncateCmd = connection.CreateCommand())
            {
                truncateCmd.Transaction = dbTransaction;
                truncateCmd.CommandText =
                    "TRUNCATE TABLE fps.proj_subcontract_staging, fps.tblstagingmonthlyoutput, fps.tblstagingmonthlytime RESTART IDENTITY;";
                await truncateCmd.ExecuteNonQueryAsync();
            }

            // In-transaction telemetry — proves the mutations genuinely took effect before we
            // deliberately abort, so the eventual rollback is proving something real.
            string? inTransactionOpenStatus;
            await using (var readCmd = connection.CreateCommand())
            {
                readCmd.Transaction = dbTransaction;
                readCmd.CommandText = "SELECT yearstatus FROM fps.tblyearmaster WHERE fpsyear = @fpsyear;";
                AddParam(readCmd, "fpsyear", _liveOpenYear);
                inTransactionOpenStatus = (string?)await readCmd.ExecuteScalarAsync();
            }

            string? inTransactionTargetStatus;
            await using (var readCmd = connection.CreateCommand())
            {
                readCmd.Transaction = dbTransaction;
                readCmd.CommandText = "SELECT yearstatus FROM fps.tblyearmaster WHERE fpsyear = @fpsyear;";
                AddParam(readCmd, "fpsyear", targetYear);
                inTransactionTargetStatus = (string?)await readCmd.ExecuteScalarAsync();
            }

            var inTransactionStagingCounts = await GetStagingRowCountsOnConnectionAsync(connection, dbTransaction);

            // Mirrors "failure between the two year updates" / "failure before commit" from the
            // agreed CutOver contract — abort instead of committing.
            await transaction.RollbackAsync();

            Assert.Equal("Closed", inTransactionOpenStatus);
            Assert.Equal("Open", inTransactionTargetStatus);
            Assert.Equal(0, inTransactionStagingCounts.subcontract);
            Assert.Equal(0, inTransactionStagingCounts.output);
            Assert.Equal(0, inTransactionStagingCounts.time);

            // Post-rollback: original state restored, including staging content and identity.
            var (postRollbackOpenStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (postRollbackTargetStatus, _) = await GetYearStateAsync(targetYear);
            Assert.Equal("Open", postRollbackOpenStatus);
            Assert.Equal("Planned", postRollbackTargetStatus);

            var (postRollbackSubcontractCount, postRollbackOutputCount, postRollbackTimeCount) = await GetStagingRowCountsAsync();
            Assert.Equal(baselineSubcontractCount, postRollbackSubcontractCount);
            Assert.Equal(baselineOutputCount, postRollbackOutputCount);
            Assert.Equal(baselineTimeCount, postRollbackTimeCount);

            // Identity/sequence state also rolled back — a fresh insert should NOT start at 1,
            // since RESTART IDENTITY itself was undone by ROLLBACK (unlike a live nextval() call,
            // TRUNCATE ... RESTART IDENTITY's reset genuinely is transactional in Postgres).
            var postRollbackNewId = await InsertProbeMonthlyTimeRowAsync();
            Assert.True(postRollbackNewId > 1, "Expected the sequence to have continued past 1 — RESTART IDENTITY should have been rolled back.");
        }
        finally
        {
            await DeleteYearAsync(targetYear);
            await TruncateStagingTablesAsync();
        }
    }

    // ─── Real worker/orchestrator path — end to end through JobOrchestrator + BatchJobFactory ─

    /// <summary>
    /// The only test in this class that goes through the real <see cref="JobOrchestrator"/> +
    /// <see cref="BatchJobFactory"/> + <see cref="YearEndCutoverJobHandler"/> path, exactly as the
    /// worker container would run it for an Approved job: claim, shared lock, canonical handler
    /// resolution, the real business pipeline, lock release, and the final job_queue status —
    /// rather than calling <see cref="YearEndCutoverService.ExecuteAsync"/> directly as every other
    /// test in this class does. Sets <c>BATCH_JOB_PARAMETERS_JSON</c> for real, since
    /// <see cref="YearEndExecutionContext.FromEnvironment"/> reads it directly rather than through
    /// the orchestrator's own <c>parametersJson</c> argument.
    /// </summary>
    [SkippableFact]
    public async Task RunAsync_ThroughRealOrchestratorAndFactory_ExecutesCutoverEndToEndAndClearsStaging()
    {
        SkipUnlessReady();

        var targetYear = _liveOpenYear + 510;
        var dataSetupJobQueueId = Guid.NewGuid();
        var cutoverJobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await SeedYearAsync(targetYear, "Planned", active: true);
        await SeedDataSetupExecutionAsync(targetYear, dataSetupJobQueueId, "Completed");
        await SeedStagingRowsAsync();
        await SeedApprovedCutoverJobQueueRowAsync(jobExecutionId, cutoverJobQueueId, targetYear);

        var originalParametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        var parametersJson = $"{{\"plannedYear\":\"{targetYear}\"}}";
        Environment.SetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON", parametersJson);

        try
        {
            await using var orchestratorContext = CreateDbContext();
            var orchestrator = BuildRealOrchestrator(orchestratorContext);

            var result = await orchestrator.RunAsync(
                BatchJobNames.YearEndCutover,
                RunMode.Manual,
                jobExecutionId,
                "integration-test-approver",
                null,
                parametersJson);

            Assert.Equal(JobStatus.Completed, result.Status);

            var (originalOpenYearStatus, _) = await GetYearStateAsync(_liveOpenYear);
            var (targetStatus, _) = await GetYearStateAsync(targetYear);
            Assert.Equal("Closed", originalOpenYearStatus);
            Assert.Equal("Open", targetStatus);

            var (subcontractCount, outputCount, timeCount) = await GetStagingRowCountsAsync();
            Assert.Equal(0, subcontractCount);
            Assert.Equal(0, outputCount);
            Assert.Equal(0, timeCount);

            await using var verifyContext = CreateDbContext();
            var finalStatus = await verifyContext.Database
                .SqlQuery<string>($@"
                    SELECT s.status AS ""Value""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {cutoverJobQueueId}")
                .SingleAsync();
            Assert.Equal("Completed", finalStatus);

            var lockRepository = new BatchLockRepository(verifyContext);
            var activeLock = await lockRepository.GetActiveLockAsync(BatchJobNames.YearEndLock);
            Assert.Null(activeLock);
        }
        finally
        {
            Environment.SetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON", originalParametersJson);

            await using var restoreContext = CreateDbContext();
            await restoreContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = {_liveOpenYear};");

            await using (var cleanupContext = CreateDbContext())
            {
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.job_lock WHERE jobqueueid = {cutoverJobQueueId};");
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.job_queue_log WHERE jobqueueid = {cutoverJobQueueId};");
                await cleanupContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM fps.job_queue WHERE jobqueueid = {cutoverJobQueueId};");
            }

            await DeleteYearAsync(targetYear);
            await DeleteJobQueueRowAsync(dataSetupJobQueueId);
            await TruncateStagingTablesAsync();
        }
    }

    private async Task SeedApprovedCutoverJobQueueRowAsync(Guid jobExecutionId, Guid jobQueueId, int targetYear)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndCutover}")
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
                 {targetYear}, 'integration-test-approver', NOW());");
    }

    private JobOrchestrator BuildRealOrchestrator(BatchJobsDbContext context)
    {
        var services = new ServiceCollection();
        services.AddSingleton(CreateDbContextFactory());
        services.AddSingleton<IYearEndCutoverService, YearEndCutoverService>();
        services.AddSingleton<ILogger<YearEndCutoverService>>(NullLogger<YearEndCutoverService>.Instance);
        services.AddSingleton(Substitute.For<ICorrelationService>());
        services.AddSingleton<ILogger<YearEndCutoverJobHandler>>(NullLogger<YearEndCutoverJobHandler>.Instance);
        services.AddSingleton<YearEndCutoverJobHandler>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);
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

    // ─── Helpers ────────────────────────────────────────────────────────────────────────

    private void SkipUnlessReady()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndDataSetupCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Completed'/'Failed' is not yet provisioned on this database.");
        Skip.IfNot(
            _exactlyOneOpenYear,
            "fps.tblyearmaster does not have exactly one Open year on this database. " +
            "This test must target an isolated/local CR048-aligned database — never batchjob_testing.");
    }

    private YearEndCutoverService CreateService() =>
        new(CreateDbContextFactory(), NullLogger<YearEndCutoverService>.Instance);

    private async Task SeedYearAsync(int fpsYear, string yearStatus, bool active)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.tblyearmaster WHERE fpsyear = {fpsYear};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES ({fpsYear}, {$"IT{fpsYear}"}, {yearStatus}, 'YearEndCutoverServiceIntegrationTests', {active}, 'IntegrationTest');");
    }

    private async Task SeedDataSetupExecutionAsync(int targetYear, Guid jobQueueId, string status)
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

    private async Task SeedStagingRowsAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.proj_subcontract_staging
                (project, testjob, month, amount, workgroup, acctcode, supplier, description,
                 suppliernumber, dailyrate, animaldays, filename, importedby, importeddate,
                 validationfailure, isexported, ispassed)
            VALUES
                ('IT-PROJ', 'IT-JOB', '1', '100', 'IT-WG', 'IT-ACCT', 'IT-SUPPLIER', 'seed row',
                 '1', '10', '1', 'seed.xlsx', 'IntegrationTest', NOW(), NULL, false, false);");

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblstagingmonthlyoutput
                (testcode, buyer, month, workgroup, volume, failurecomments, passed, filename, importedby, importeddate)
            VALUES
                ('IT-TEST', 'IT-BUYER', 1, 'IT-WG', 1, NULL, false, 'seed.xlsx', 'IntegrationTest', NOW());");

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblstagingmonthlytime
                (pactstaffid, timecode, parentproject, month, workgroup, hours, failurecomments, passed,
                 pactid, newworkgroup, oldtestcode, name, filename, importedby, importeddate)
            VALUES
                ('IT-STAFF', 'IT-TC', 'IT-PROJ', 1, 'IT-WG', 1, NULL, false,
                 'IT-PACT', 'IT-WG', 'IT-OLDTC', 'IT-NAME', 'seed.xlsx', 'IntegrationTest', NOW());");
    }

    private async Task<(int Subcontract, int MonthlyOutput, int MonthlyTime)> GetStagingRowCountsAsync()
    {
        await using var context = CreateDbContext();

        var subcontract = await context.Database
            .SqlQuery<int>($@"SELECT count(*)::int AS ""Value"" FROM fps.proj_subcontract_staging")
            .SingleAsync();
        var output = await context.Database
            .SqlQuery<int>($@"SELECT count(*)::int AS ""Value"" FROM fps.tblstagingmonthlyoutput")
            .SingleAsync();
        var time = await context.Database
            .SqlQuery<int>($@"SELECT count(*)::int AS ""Value"" FROM fps.tblstagingmonthlytime")
            .SingleAsync();

        return (subcontract, output, time);
    }

    private static async Task<(int subcontract, int output, int time)> GetStagingRowCountsOnConnectionAsync(
        System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
    {
        async Task<int> CountAsync(string table)
        {
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SELECT count(*)::int FROM {table};";
            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        return (
            await CountAsync("fps.proj_subcontract_staging"),
            await CountAsync("fps.tblstagingmonthlyoutput"),
            await CountAsync("fps.tblstagingmonthlytime"));
    }

    private async Task TruncateStagingTablesAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE fps.proj_subcontract_staging, fps.tblstagingmonthlyoutput, fps.tblstagingmonthlytime RESTART IDENTITY;");
    }

    // INSERT ... RETURNING is not composable SQL, so EF's SqlQuery<T> (which wraps the SQL as a
    // subquery) rejects it — these go through a raw ADO.NET command instead, same style already
    // used throughout YearEndCutoverService itself.

    private async Task<int> InsertProbeSubcontractRowAsync() =>
        await ExecuteInsertReturningIdAsync(
            "INSERT INTO fps.proj_subcontract_staging (project, testjob, month, amount, workgroup) " +
            "VALUES ('PROBE', 'PROBE', '1', '1', 'PROBE') RETURNING id;");

    private async Task<int> InsertProbeMonthlyOutputRowAsync() =>
        await ExecuteInsertReturningIdAsync(
            "INSERT INTO fps.tblstagingmonthlyoutput (testcode, buyer, month, workgroup) " +
            "VALUES ('PROBE', 'PROBE', 1, 'PROBE') RETURNING id;");

    private async Task<int> InsertProbeMonthlyTimeRowAsync() =>
        await ExecuteInsertReturningIdAsync(
            "INSERT INTO fps.tblstagingmonthlytime (pactstaffid, timecode, parentproject, month, workgroup, hours, passed, pactid) " +
            "VALUES ('PROBE', 'PROBE', 'PROBE', 1, 'PROBE', 1, false, 'PROBE') RETURNING id;");

    private async Task<int> ExecuteInsertReturningIdAsync(string commandText)
    {
        await using var context = CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        return (int)(await command.ExecuteScalarAsync())!;
    }

    private static void AddParam(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
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
