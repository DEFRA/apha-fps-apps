using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed tests for <see cref="JobExecutionRepository.GetExecutionByJobQueueIdAsync"/>,
/// added for Phase 1 of orphan lock reconciliation — reconciliation only knows the JobQueueId
/// referenced by an expired job_lock row, not the JobExecutionId. See
/// batchjobs-job-lock-lease-heartbeat-worker-implementation-spec-2026-09-18.md.
/// </summary>
[Trait("Category", "Integration")]
public sealed class JobExecutionRepositoryGetByJobQueueIdIntegrationTests : IAsyncLifetime
{
    private readonly string _connectionString;
    private string? _skipReason;

    public JobExecutionRepositoryGetByJobQueueIdIntegrationTests()
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

    [SkippableFact]
    public async Task GetExecutionByJobQueueIdAsync_WhenRowExists_ReturnsMatchingRecordWithRequestedByPreserved()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();
        const string requestedBy = "get-by-jobqueueid-test-requester";

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.RecreateSummary}")
                .SingleAsync();

            var runningStatusId = await context.Database
                .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Running'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {runningStatusId}, {requestedBy}, NOW(), NOW());");
        }

        try
        {
            var repository = CreateRepository();

            var execution = await repository.GetExecutionByJobQueueIdAsync(jobQueueId);

            Assert.NotNull(execution);
            Assert.Equal(jobQueueId, execution!.JobQueueId);
            Assert.Equal(jobExecutionId, execution.JobExecutionId);
            Assert.Equal(BatchJobNames.RecreateSummary, execution.JobName);
            Assert.Equal(JobStatus.Running, execution.Status);
            // UserId must round-trip from the row's own RequestedBy — reconciliation (Phase 2)
            // relies on this to pass the original requester straight through to
            // UpdateExecutionRecordAsync so it doesn't overwrite RequestedBy with a synthetic value.
            Assert.Equal(requestedBy, execution.UserId);
        }
        finally
        {
            await CleanupAsync(jobQueueId);
        }
    }

    [SkippableFact]
    public async Task GetExecutionByJobQueueIdAsync_WhenNoRowExists_ReturnsNull()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var repository = CreateRepository();

        var execution = await repository.GetExecutionByJobQueueIdAsync(Guid.NewGuid());

        Assert.Null(execution);
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
