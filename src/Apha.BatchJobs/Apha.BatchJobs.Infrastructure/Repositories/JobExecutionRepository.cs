using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// Implementation of job execution repository using EF Core.
/// </summary>
public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<JobExecutionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the JobExecutionRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for structured execution record events.</param>
    public JobExecutionRepository(BatchJobsDbContext context, ILogger<JobExecutionRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? NullLogger<JobExecutionRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var now = DateTime.UtcNow;
        _logger.LogInformation(
            "Create execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status} | FpsYear={FpsYear}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status,
            record.FpsYear);

        // Check if an Initiated row already exists for this jobExecutionId (manual job path)
        var existingRow = await _context.TblJobQueue
            .FirstOrDefaultAsync(q => q.JobExecutionId == record.JobExecutionId, cancellationToken);

        if (existingRow != null)
        {
            // UPDATE the Initiated row to Running
            var statusId = await EnsureStatusAsync(existingRow.JobId, record.Status.ToString(), cancellationToken);
            var previousStatus = existingRow.StatusId;
            existingRow.StatusId = statusId;
            existingRow.StartDateTime = record.StartedAt;
            existingRow.RequestedBy = record.UserId;
            if (record.FpsYear.HasValue)
            {
                existingRow.FpsYear = record.FpsYear.Value;
            }
            existingRow.UpdatedAt = now;

            // Ensure the record's JobQueueId matches the persisted row
            record.JobQueueId = existingRow.JobQueueId;

            _context.TblJobQueueLog.Add(new TblJobQueueLog
            {
                JobQueueId = existingRow.JobQueueId,
                StatusId = statusId,
                PerformedBy = record.UserId,
                LogTime = now,
                Note = "Worker started execution - Initiated → Running"
            });

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[Worker → DB] ✓ Initiated → Running transition complete | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | StartDateTime={StartDateTime}",
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.StartedAt);

            return 0;
        }

        _logger.LogError(
            "Execution start rejected: no pre-created Initiated row | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId);

        throw new InvalidOperationException(
            $"No pre-created Initiated record exists for JobExecutionId {record.JobExecutionId}. API must insert Initiated before worker execution starts.");
    }

    /// <inheritdoc />
    public async Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        // The worker uses a shared DbContext across repositories. Do not clear ChangeTracker here to avoid nested transaction errors.

        _logger.LogInformation(
            "Update execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        var queueRow = await _context.TblJobQueue
            .FirstOrDefaultAsync(q => q.JobQueueId == record.JobQueueId, cancellationToken);

        if (queueRow == null)
        {
            _logger.LogInformation(
                "Execution record not found for update | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId}",
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.UserId);
            return;
        }

        var now = DateTime.UtcNow;
        var statusId = await EnsureStatusAsync(queueRow.JobId, record.Status.ToString(), cancellationToken);

        queueRow.StatusId = statusId;
        queueRow.RequestedBy = record.UserId;
        queueRow.EndDateTime = record.CompletedAt;
        queueRow.ErrorMessage = record.ErrorMessage;
        queueRow.UpdatedAt = now;

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = record.JobQueueId,
            StatusId = statusId,
            PerformedBy = record.UserId,
            LogTime = now,
            Note = BuildStatusNote(record.Status)
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Execution record updated | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var last = await (
            from q in _context.TblJobQueue
            join m in _context.TblJobMaster on q.JobId equals m.JobId
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where m.JobName == jobName
            orderby q.StartDateTime descending
            select new
            {
                m.JobName,
                q.JobExecutionId,
                q.JobQueueId,
                q.RequestedBy,
                q.RequestedAtUtc,
                q.FpsYear,
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (last == null)
            return null;

        var parsedStatus = Enum.TryParse<JobStatus>(last.Status, true, out var status)
            ? status
            : JobStatus.Failed;

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = last.JobName,
            JobExecutionId = last.JobExecutionId,
            JobQueueId = last.JobQueueId,
            UserId = last.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = parsedStatus,
            RequestedAtUtc = last.RequestedAtUtc,
            FpsYear = last.FpsYear,
            StartedAt = last.StartDateTime ?? DateTime.UtcNow,
            CompletedAt = last.EndDateTime,
            DurationSeconds = last.EndDateTime.HasValue && last.StartDateTime.HasValue
                ? (int)(last.EndDateTime.Value - last.StartDateTime.Value).TotalSeconds
                : null,
            ErrorMessage = last.ErrorMessage,
            RetryAttempts = 0
        };
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetExecutionByJobExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        var execution = await (
            from q in _context.TblJobQueue
            join m in _context.TblJobMaster on q.JobId equals m.JobId
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobExecutionId == jobExecutionId
            orderby q.StartDateTime descending
            select new
            {
                m.JobName,
                q.JobExecutionId,
                q.JobQueueId,
                q.RequestedBy,
                q.RequestedAtUtc,
                q.FpsYear,
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (execution == null)
            return null;

        if (!Enum.TryParse<JobStatus>(execution.Status, true, out var status))
        {
            _logger.LogError(
                "Status parsing failed for JobExecutionId | JobExecutionId={JobExecutionId} | StatusFromDb={StatusValue}",
                execution.JobExecutionId,
                execution.Status);
            throw new InvalidOperationException(
                $"Invalid status '{execution.Status}' in database for JobExecutionId '{execution.JobExecutionId}'.");
        }

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = execution.JobName,
            JobExecutionId = execution.JobExecutionId,
            JobQueueId = execution.JobQueueId,
            UserId = execution.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = status,
            RequestedAtUtc = execution.RequestedAtUtc,
            FpsYear = execution.FpsYear,
            StartedAt = execution.StartDateTime ?? DateTime.UtcNow,
            CompletedAt = execution.EndDateTime,
            DurationSeconds = execution.EndDateTime.HasValue && execution.StartDateTime.HasValue
                ? (int)(execution.EndDateTime.Value - execution.StartDateTime.Value).TotalSeconds
                : null,
            ErrorMessage = execution.ErrorMessage,
            RetryAttempts = 0
        };
    }

    /// <inheritdoc />
    public async Task<Guid> CreateInitiatedRecordAsync(
        string jobName,
        Guid jobExecutionId,
        string requestedBy,
        DateTime requestedAtUtc,
        RunMode runMode,
        CancellationToken cancellationToken = default,
        int? fpsYear = null)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name is required.", nameof(jobName));
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));

        var jobId = await EnsureJobMasterAsync(jobName, cancellationToken);
        var statusId = await EnsureStatusAsync(jobId, nameof(JobStatus.Initiated), cancellationToken);

        var jobQueueId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _context.TblJobQueue.Add(new TblJobQueue
        {
            JobQueueId = jobQueueId,
            JobExecutionId = jobExecutionId,
            JobId = jobId,
            StatusId = statusId,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc,
            FpsYear = fpsYear,
            // Keep compatibility with environments where startdatetime is still NOT NULL.
            // Status remains Initiated; worker updates the value when transitioning to Running.
            StartDateTime = requestedAtUtc,
            EndDateTime = null,
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        });

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = jobQueueId,
            StatusId = statusId,
            PerformedBy = requestedBy,
            LogTime = now,
            Note = "Job accepted by API - Initiated"
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[API → DB] ✓ Initiated record created | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | RunMode={RunMode} | RequestedBy={RequestedBy} | RequestedAtUtc={RequestedAtUtc} | FpsYear={FpsYear}",
            jobName, jobExecutionId, jobQueueId, runMode, requestedBy, requestedAtUtc, fpsYear);

        return jobQueueId;
    }

    private async Task<int> EnsureJobMasterAsync(string jobName, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobMaster
            .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (existing != null)
            return existing.JobId;

        throw new InvalidOperationException(
            $"Job master record not found for job name '{jobName}'. " +
            $"Job must be created and configured in the database before execution.");
    }

    private async Task<int> EnsureStatusAsync(int jobId, string status, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobStatus
            .FirstOrDefaultAsync(s => s.JobId == jobId && s.Status == status, cancellationToken);

        if (existing != null)
            return existing.StatusId;

        throw new InvalidOperationException(
            $"Job status row not found for JobId '{jobId}' and Status '{status}'. " +
            "Status catalog must be provisioned via approved DBA migration/CR scripts before worker execution.");
    }

    private static string BuildStatusNote(JobStatus status) => status switch
    {
        JobStatus.Completed => "Execution completed",
        JobStatus.Failed => "Execution failed",
        JobStatus.Cancelled => "Execution cancelled",
        _ => $"Status changed to {status}"
    };
}
