using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed tests for <see cref="JobExecutionRepository.MarkFailedIfNonTerminalAsync"/> —
/// the atomic conditional update orphan lock reconciliation uses so a worker that independently
/// reaches a terminal status can never be overwritten by a synthetic "lease expired" Failed. Proves
/// the same live-recheck-in-the-write-statement guarantee that
/// <c>BatchLockRepositoryTests.DeleteIfStillExpiredAsync</c> already proves for job_lock.
/// </summary>
[Trait("Category", "Integration")]
public sealed class JobExecutionRepositoryMarkFailedIfNonTerminalIntegrationTests : IAsyncLifetime
{
    private const string ErrorMessage = "Execution marked Failed because its worker lock lease expired.";
    private const string DiagnosticSummary = "Orphaned execution reconciled after job_lock lease expiry.";

    private readonly string _connectionString;
    private string? _skipReason;

    public JobExecutionRepositoryMarkFailedIfNonTerminalIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? string.Empty;
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
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableTheory]
    [InlineData(JobStatus.Initiated)]
    [InlineData(JobStatus.Approved)]
    [InlineData(JobStatus.Running)]
    public async Task MarkFailedIfNonTerminalAsync_WhenNonTerminal_FlipsToFailedWritesLogAndReturnsTrue(JobStatus nonTerminalStatus)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const string originalRequestedBy = "mark-failed-nonterminal-requester";
        var (jobQueueId, jobId) = await SeedJobQueueRowAsync(nonTerminalStatus, originalRequestedBy);

        try
        {
            var repository = CreateRepository();

            var result = await repository.MarkFailedIfNonTerminalAsync(jobQueueId, ErrorMessage, DiagnosticSummary);

            Assert.True(result);

            await using var context = CreateDbContext();
            var persisted = await context.Database
                .SqlQuery<PersistedRow>($@"
                    SELECT s.status AS ""Status"", q.errormessage AS ""ErrorMessage"", q.requestedby AS ""RequestedBy""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(nameof(JobStatus.Failed), persisted.Status);
            Assert.Equal(ErrorMessage, persisted.ErrorMessage);
            // RequestedBy is never touched by the conditional update's setters — proves it
            // survives structurally, not just by being copied through.
            Assert.Equal(originalRequestedBy, persisted.RequestedBy);

            var logNote = await context.Database
                .SqlQuery<string>($@"
                    SELECT note AS ""Value"" FROM fps.job_queue_log
                    WHERE jobqueueid = {jobQueueId} AND statusid = (SELECT statusid FROM fps.job_status WHERE jobid = {jobId} AND status = 'Failed')
                    ORDER BY logtime DESC LIMIT 1")
                .SingleAsync();
            Assert.Equal(DiagnosticSummary, logNote);
        }
        finally
        {
            await CleanupAsync(jobQueueId);
        }
    }

    [SkippableTheory]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    [InlineData(JobStatus.Rejected)]
    public async Task MarkFailedIfNonTerminalAsync_WhenAlreadyTerminal_ReturnsFalseAndLeavesRowUnchanged(JobStatus terminalStatus)
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var (jobQueueId, _) = await SeedJobQueueRowAsync(terminalStatus, "mark-failed-terminal-requester");

        try
        {
            var repository = CreateRepository();

            var result = await repository.MarkFailedIfNonTerminalAsync(jobQueueId, ErrorMessage, DiagnosticSummary);

            Assert.False(result);

            await using var context = CreateDbContext();
            var persistedStatus = await context.Database
                .SqlQuery<string>($@"
                    SELECT s.status AS ""Value""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(terminalStatus.ToString(), persistedStatus);
        }
        finally
        {
            await CleanupAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task MarkFailedIfNonTerminalAsync_WhenNoMatchingRow_ReturnsFalse()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var repository = CreateRepository();

        var result = await repository.MarkFailedIfNonTerminalAsync(Guid.NewGuid(), ErrorMessage, DiagnosticSummary);

        Assert.False(result);
    }

    [SkippableFact]
    public async Task MarkFailedIfNonTerminalAsync_WhenRowBecomesTerminalIndependentlyBeforeTheCall_ReturnsFalseAndLeavesCompletedIntact()
    {
        // The exact regression this method exists to close: a real worker reaching Completed on
        // its own between whatever observed the row as non-terminal and this atomic write must
        // never be overwritten. Flipping the row to Completed via raw SQL before calling the
        // repository method simulates that independent completion.
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var (jobQueueId, jobId) = await SeedJobQueueRowAsync(JobStatus.Running, "mark-failed-race-requester");

        await using (var context = CreateDbContext())
        {
            var completedStatusId = await context.Database
                .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Completed'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.job_queue SET statusid = {completedStatusId}, enddatetime = NOW()
                WHERE jobqueueid = {jobQueueId};");
        }

        try
        {
            var repository = CreateRepository();

            var result = await repository.MarkFailedIfNonTerminalAsync(jobQueueId, ErrorMessage, DiagnosticSummary);

            Assert.False(result);

            await using var context = CreateDbContext();
            var persisted = await context.Database
                .SqlQuery<PersistedRow>($@"
                    SELECT s.status AS ""Status"", q.errormessage AS ""ErrorMessage"", q.requestedby AS ""RequestedBy""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(nameof(JobStatus.Completed), persisted.Status);
            Assert.Null(persisted.ErrorMessage);
        }
        finally
        {
            await CleanupAsync(jobQueueId);
        }
    }

    private sealed record PersistedRow(string Status, string? ErrorMessage, string RequestedBy);

    private async Task<(Guid JobQueueId, int JobId)> SeedJobQueueRowAsync(JobStatus status, string requestedBy)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.BulkTestRatesUpdate}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = {status.ToString()}")
            .SingleAsync();

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
            VALUES
                ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, {requestedBy}, NOW(), NOW());");

        return (jobQueueId, jobId);
    }

    private async Task CleanupAsync(Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
    }

    private JobExecutionRepository CreateRepository() => new(CreateDbContext(), NullLogger<JobExecutionRepository>.Instance);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
