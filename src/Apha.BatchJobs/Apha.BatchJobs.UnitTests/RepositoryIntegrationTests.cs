using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;

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
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? DefaultConnectionString;
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

    [Fact]
    public async Task TryAcquireLockAsync_FirstSucceeds_SecondReturnsFalse_ForSameJob()
    {
        if (!CanRunIntegrationTests())
            return;

        await using var context = CreateDbContext();
        var repository = new BatchLockRepository(context);

        var first = await repository.TryAcquireLockAsync("IntegrationLockJob", Guid.NewGuid().ToString("N"), 300);
        var second = await repository.TryAcquireLockAsync("IntegrationLockJob", Guid.NewGuid().ToString("N"), 300);

        first.ShouldBeTrue();
        second.ShouldBeFalse();
    }

    [Fact]
    public async Task ReleaseLockAsync_RemovesHeldLock()
    {
        if (!CanRunIntegrationTests())
            return;

        var runId = Guid.NewGuid().ToString("N");

        await using (var context = CreateDbContext())
        {
            var repository = new BatchLockRepository(context);
            var acquired = await repository.TryAcquireLockAsync("IntegrationReleaseJob", runId, 300);
            acquired.ShouldBeTrue();
            await repository.ReleaseLockAsync("IntegrationReleaseJob", runId);
        }

        await using var verifyContext = CreateDbContext();
        var active = await verifyContext.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == "IntegrationReleaseJob");

        active.ShouldBeNull();
    }

    [Fact]
    public async Task CreateExecutionRecordAsync_WritesQueueAndLogRows()
    {
        if (!CanRunIntegrationTests())
            return;

        var runId = Guid.NewGuid().ToString("N");
        var record = new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = "IntegrationExecutionJob",
            RunId = runId,
            JobType = JobType.Unknown,
            RunMode = RunMode.AdHoc,
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
            "SELECT COUNT(*) FROM operational.tbljobqueue WHERE jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", Guid.Parse(runId)));

        var logRows = await ScalarIntAsync(
            "SELECT COUNT(*) FROM operational.tbljobqueue_log ql INNER JOIN operational.tbljobqueue q ON q.jobqueueid = ql.jobqueueid WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", Guid.Parse(runId)));

        queueRows.ShouldBe(1);
        logRows.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateExecutionRecordAsync_UpdatesStatusAndAppendsLog()
    {
        if (!CanRunIntegrationTests())
            return;

        var runId = Guid.NewGuid().ToString("N");
        var startedAt = DateTime.UtcNow.AddMinutes(-1);

        await using (var context = CreateDbContext())
        {
            var repository = new JobExecutionRepository(context);

            await repository.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = "IntegrationUpdateJob",
                RunId = runId,
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
                RunId = runId,
                JobType = JobType.Unknown,
                RunMode = RunMode.Scheduled,
                Status = JobStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                RetryAttempts = 0
            });
        }

        var statusName = await ScalarStringAsync(
            "SELECT s.status FROM operational.tbljobqueue q INNER JOIN operational.tbljobstatus s ON s.statusid = q.statusid WHERE q.jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", Guid.Parse(runId)));

        var logCount = await ScalarIntAsync(
            "SELECT COUNT(*) FROM operational.tbljobqueue_log WHERE jobqueueid = @runId::uuid",
            new NpgsqlParameter("runId", Guid.Parse(runId)));

        statusName.ShouldBe("Completed");
        logCount.ShouldBe(2);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private async Task EnsureSchemaAsync()
    {
        const string sql = @"
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS operational;

CREATE TABLE IF NOT EXISTS operational.batch_lock (
    lock_id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name VARCHAR(255) NOT NULL,
    acquired_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    run_id VARCHAR(64) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_batch_lock_job_name ON operational.batch_lock (job_name);
CREATE INDEX IF NOT EXISTS idx_batch_lock_job_name_active ON operational.batch_lock (job_name, is_active);
CREATE INDEX IF NOT EXISTS idx_batch_lock_expires_at ON operational.batch_lock (expires_at);
CREATE UNIQUE INDEX IF NOT EXISTS uq_batch_lock_job_name_active
    ON operational.batch_lock (job_name)
    WHERE is_active = TRUE;

CREATE TABLE IF NOT EXISTS operational.tbljobmaster (
    jobid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname VARCHAR(100) NOT NULL UNIQUE,
    frequency VARCHAR(50),
    note VARCHAR(250),
    timetolive INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_tbljobmaster_timetolive_positive CHECK (timetolive > 0)
);

CREATE TABLE IF NOT EXISTS operational.tbljobstatus (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_tbljobstatus_jobid
        FOREIGN KEY (jobid)
        REFERENCES operational.tbljobmaster(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_tbljobstatus_jobid_status UNIQUE (jobid, status)
);

CREATE TABLE IF NOT EXISTS operational.tbljobqueue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_tbljobqueue_jobid
        FOREIGN KEY (jobid)
        REFERENCES operational.tbljobmaster(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_tbljobqueue_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.tbljobstatus(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_tbljobqueue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);

CREATE TABLE IF NOT EXISTS operational.tbljobqueue_log (
    jobqueuelogid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid UUID NOT NULL,
    statusid INTEGER NOT NULL,
    performedby VARCHAR(100) NOT NULL,
    logtime TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note VARCHAR(500),
    CONSTRAINT fk_tbljobqueue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES operational.tbljobqueue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_tbljobqueue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.tbljobstatus(statusid)
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
    operational.batch_lock,
    operational.tbljobqueue_log,
    operational.tbljobqueue,
    operational.tbljobstatus,
    operational.tbljobmaster
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
