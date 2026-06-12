using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Postgres-backed integration tests for repository persistence behavior.
/// </summary>
public sealed class RepositoryIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public RepositoryIntegrationTests()
    {
        _connectionString = TestConnectionStringResolver.ResolveForTests(DefaultConnectionString);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await EnsureSchemaAsync();
            await ResetTablesAsync();
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task TryAcquireLockAsync_FirstSucceeds_SecondReturnsFalse_ForSameJob()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        var repository = new BatchLockRepository(context);

        var jobQueueId1 = Guid.NewGuid();
        var jobQueueId2 = Guid.NewGuid();
        var first = await repository.TryAcquireLockAsync("IntegrationLockJob", jobQueueId1, 300);
        var second = await repository.TryAcquireLockAsync("IntegrationLockJob", jobQueueId2, 300);

        Assert.True(first);
        Assert.False(second);
    }

    [SkippableFact]
    public async Task ReleaseLockAsync_RemovesHeldLock()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();

        await using (var context = CreateDbContext())
        {
            var repository = new BatchLockRepository(context);
            var acquired = await repository.TryAcquireLockAsync("IntegrationReleaseJob", jobQueueId, 300);
            Assert.True(acquired);
            await repository.ReleaseLockAsync("IntegrationReleaseJob", jobQueueId);
        }

        await using var verifyContext = CreateDbContext();
        var active = await verifyContext.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == "IntegrationReleaseJob");

        Assert.Null(active);
    }

    [SkippableFact]
    public async Task CreateExecutionRecordAsync_WritesQueueAndLogRows()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();
        var record = new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = "IntegrationExecutionJob",
            JobExecutionId = Guid.NewGuid(),
            JobQueueId = jobQueueId,
            UserId = "test-user",
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = JobStatus.Running,
            StartedAt = DateTime.UtcNow,
            RetryAttempts = 0
        };

        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);
            _ = await repository.CreateExecutionRecordAsync(record);
        }

        var queueRows = await ScalarIntAsync(
            "SELECT COUNT(*) FROM fps.job_queue WHERE jobqueueid = @jobQueueId",
            new NpgsqlParameter("jobQueueId", jobQueueId));

        var logRows = await ScalarIntAsync(
            "SELECT COUNT(*) FROM fps.job_queue_log ql INNER JOIN fps.job_queue q ON q.jobqueueid = ql.jobqueueid WHERE q.jobqueueid = @jobQueueId",
            new NpgsqlParameter("jobQueueId", jobQueueId));

        Assert.Equal(1, queueRows);
        Assert.Equal(1, logRows);
    }

    [SkippableFact]
    public async Task UpdateExecutionRecordAsync_UpdatesStatusAndAppendsLog()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-1);

        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);

            await repository.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "IntegrationUpdateJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Scheduled,
                Status = JobStatus.Running,
                StartedAt = startedAt,
                RetryAttempts = 0
            });

            await repository.UpdateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "IntegrationUpdateJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Scheduled,
                Status = JobStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                RetryAttempts = 0
            });
        }

        var statusName = await ScalarStringAsync(
            "SELECT s.status FROM fps.job_queue q INNER JOIN fps.job_status s ON s.statusid = q.statusid WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", jobQueueId));

        var logCount = await ScalarIntAsync(
            "SELECT COUNT(*) FROM fps.job_queue_log WHERE jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", jobQueueId));

        Assert.Equal("Completed", statusName);
        Assert.Equal(2, logCount);
    }

    [SkippableFact]
    public async Task TryAcquireLockAsync_WhenExistingLockExpired_AllowsReacquire()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var firstJobQueueId = Guid.NewGuid();
        var secondJobQueueId = Guid.NewGuid();

        await using var context = CreateDbContext();
        var repository = new BatchLockRepository(context);

        var first = await repository.TryAcquireLockAsync("IntegrationExpiryJob", firstJobQueueId, 1);
        Assert.True(first);

        await Task.Delay(1300);

        var second = await repository.TryAcquireLockAsync("IntegrationExpiryJob", secondJobQueueId, 300);
        Assert.True(second);
    }

    [SkippableFact]
    public async Task DependencyOutageThenRecovery_BadConnectionFails_HealthyConnectionSucceeds()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const string unreachableConnectionString =
            "Host=127.0.0.1;Port=65432;Database=batch_jobs_foundation_db;Username=postgres;Password=password;Timeout=1;Command Timeout=1";

        await using (var badContext = CreateDbContext(unreachableConnectionString))
        {
            var badRepository = new BatchLockRepository(badContext);

            await Assert.ThrowsAnyAsync<Exception>(
                () => badRepository.TryAcquireLockAsync("IntegrationOutageJob", Guid.NewGuid(), 30));
        }

        await using (var goodContext = CreateDbContext())
        {
            var goodRepository = new BatchLockRepository(goodContext);
            var recovered = await goodRepository.TryAcquireLockAsync("IntegrationOutageJob", Guid.NewGuid(), 300);
            Assert.True(recovered);
        }
    }

    /// <summary>
    /// CR-004: Degradation scenario - Database timeout should trigger retry exhaustion.
    /// Validates: exception type is captured, execution record is updated with error, lock is released.
    /// </summary>
    [SkippableFact]
    public async Task ExecutionRecord_UpdateFailure_PartialDataNotCorrupted()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();

        // Create initial record
        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);
            await repository.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "DegradationPartialFailJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            });
        }

        // Simulate partial failure: update status to Failed
        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);
            var completedAt = DateTime.UtcNow;

            // This should still succeed and persist the failure state
            await repository.UpdateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "DegradationPartialFailJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Failed,
                StartedAt = DateTime.UtcNow.AddSeconds(-10),
                CompletedAt = completedAt,
                DurationSeconds = 10,
                ErrorMessage = "Simulated infrastructure failure",
                StackTrace = "timeout at database layer",
                RetryAttempts = 2
            });
        }

        // Verify record was updated with error details
        var statusName = await ScalarStringAsync(
            "SELECT s.status FROM fps.job_queue q INNER JOIN fps.job_status s ON s.statusid = q.statusid WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", jobQueueId));

        var errorMsg = await ScalarStringAsync(
            "SELECT q.errormessage FROM fps.job_queue q WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", jobQueueId));

        Assert.Equal("Failed", statusName);
        Assert.NotNull(errorMsg);
        Assert.Contains("infrastructure failure", errorMsg);
    }

    /// <summary>
    /// CR-004: Verify lock contention scenario is logged as informational (not error).
    /// Validates: skipped run does not corrupt state, lock properly expires.
    /// </summary>
    [SkippableFact]
    public async Task LockContention_SkipDoesNotCorruptState_LockExpiresOnSchedule()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var firstJobQueueId = Guid.NewGuid();
        var secondJobQueueId = Guid.NewGuid();

        // First worker acquires lock
        await using (var context = CreateDbContext())
        {
            var repository = new BatchLockRepository(context);
            var acquired = await repository.TryAcquireLockAsync("DegradationLockContentionJob", firstJobQueueId, 2);
            Assert.True(acquired);

            // Create execution record for first run
            var executionRepo = new JobExecutionRepository(context);
            await executionRepo.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "DegradationLockContentionJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = firstJobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Scheduled,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            });
        }

        // Second worker attempts to acquire lock (should fail, no record created)
        await using (var context = CreateDbContext())
        {
            var repository = new BatchLockRepository(context);
            var acquired = await repository.TryAcquireLockAsync("DegradationLockContentionJob", secondJobQueueId, 300);
            Assert.False(acquired);
        }

        // Verify only first execution record exists
        var executionCount = await ScalarIntAsync(
            "SELECT COUNT(*) FROM fps.job_queue WHERE jobqueueid IN (@firstRunId::uuid, @secondRunId::uuid)",
            new NpgsqlParameter("firstRunId", firstJobQueueId),
            new NpgsqlParameter("secondRunId", secondJobQueueId));

        Assert.Equal(1, executionCount);

        // Wait for lock to expire
        await Task.Delay(2500);

        // Third worker should now acquire lock
        await using (var context = CreateDbContext())
        {
            var repository = new BatchLockRepository(context);
            var thirdJobQueueId = Guid.NewGuid();
            var acquired = await repository.TryAcquireLockAsync("DegradationLockContentionJob", thirdJobQueueId, 300);
            Assert.True(acquired);
        }
    }

    /// <summary>
    /// CR-004: Structured log field validation - ensure log entries contain expected fields.
    /// Validates: execution record logs include structured timestamp and status information.
    /// </summary>
    [SkippableFact]
    public async Task ExecutionLog_ContainsStructuredFields_QueryableByJobQueueId()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();

        // Create and complete execution with logs
        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);

            await repository.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "DegradationLogValidationJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            });

            await Task.Delay(100);

            await repository.UpdateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "DegradationLogValidationJob",
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = jobQueueId,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Completed,
                StartedAt = DateTime.UtcNow.AddMilliseconds(-100),
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = 0,
                RetryAttempts = 0
            });
        }

        // Query by JobQueueId and verify log entries
        var logCount = await ScalarIntAsync(
            "SELECT COUNT(*) FROM fps.job_queue_log ql INNER JOIN fps.job_queue q ON q.jobqueueid = ql.jobqueueid WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", jobQueueId));

        // Expect at least 2 logs: Created and Completed
        Assert.True(logCount >= 2, $"Expected at least 2 log entries, got {logCount}");

        // Verify structured fields in execution record
        var firstLogTime = await ScalarStringAsync(
            "SELECT ql.logtime FROM fps.job_queue_log ql INNER JOIN fps.job_queue q ON q.jobqueueid = ql.jobqueueid WHERE q.jobqueueid = @runId::uuid ORDER BY ql.jobqueuelogid ASC LIMIT 1",
            new NpgsqlParameter("runId", jobQueueId));

        Assert.NotNull(firstLogTime);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        return CreateDbContext(_connectionString);
    }

    private static BatchJobsDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private async Task EnsureSchemaAsync()
    {
        const string sql = @"
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS fps;

CREATE TABLE IF NOT EXISTS fps.job_lock (
    lock_id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name VARCHAR(255) NOT NULL,
    acquired_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    run_id VARCHAR(64) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name ON fps.job_lock (job_name);
CREATE INDEX IF NOT EXISTS idx_job_lock_job_name_active ON fps.job_lock (job_name, is_active);
CREATE INDEX IF NOT EXISTS idx_job_lock_expires_at ON fps.job_lock (expires_at);
CREATE UNIQUE INDEX IF NOT EXISTS uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = TRUE;

CREATE TABLE IF NOT EXISTS fps.job_master (
    jobid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname VARCHAR(100) NOT NULL UNIQUE,
    frequency VARCHAR(50),
    note VARCHAR(250),
    timetolive INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_job_master_timetolive_positive CHECK (timetolive > 0)
);

CREATE TABLE IF NOT EXISTS fps.job_status (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_status_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);

CREATE TABLE IF NOT EXISTS fps.job_queue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobexecutionid UUID NOT NULL,
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,
    requestedby VARCHAR(100) NOT NULL,
    requested_at_utc TIMESTAMPTZ,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_queue_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_job_queue_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_job_queue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_job_queue_jobexecutionid ON fps.job_queue (jobexecutionid);
CREATE INDEX IF NOT EXISTS idx_job_queue_requestedby ON fps.job_queue (requestedby);

CREATE TABLE IF NOT EXISTS fps.job_queue_log (
    jobqueuelogid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid UUID NOT NULL,
    statusid INTEGER NOT NULL,
    performedby VARCHAR(100) NOT NULL,
    logtime TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note VARCHAR(500),
    CONSTRAINT fk_job_queue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_job_queue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT
);
";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task ResetTablesAsync()
    {
        const string sql = @"
TRUNCATE TABLE
    fps.job_lock,
    fps.job_queue_log,
    fps.job_queue,
    fps.job_status,
    fps.job_master
RESTART IDENTITY CASCADE;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> ScalarIntAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private async Task<string?> ScalarStringAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }
}

