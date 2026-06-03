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
    private const int DefaultTimeToLiveSeconds = 3600;

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
            "Create execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        var jobId = await EnsureJobMasterAsync(record.JobName, cancellationToken);
        var statusId = await EnsureStatusAsync(jobId, record.Status.ToString(), cancellationToken);

        var queueRow = new TblJobQueue
        {
            JobQueueId = record.JobQueueId,
            JobExecutionId = record.JobExecutionId,
            JobId = jobId,
            StatusId = statusId,
            RequestedBy = record.UserId,
            StartDateTime = record.StartedAt,
            EndDateTime = null,
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.TblJobQueue.Add(queueRow);
        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = record.JobQueueId,
            StatusId = statusId,
            PerformedBy = record.UserId,
            LogTime = now,
            Note = "Execution started"
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Execution record created | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        // Foundation queue uses GUID correlation ID. The int return is retained
        // for interface compatibility with orchestrator and tests.
        return 0;
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
            StartedAt = last.StartDateTime,
            CompletedAt = last.EndDateTime,
            DurationSeconds = last.EndDateTime.HasValue
                ? (int)(last.EndDateTime.Value - last.StartDateTime).TotalSeconds
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
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (execution == null)
            return null;

        var parsedStatus = Enum.TryParse<JobStatus>(execution.Status, true, out var status)
            ? status
            : JobStatus.Failed;

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = execution.JobName,
            JobExecutionId = execution.JobExecutionId,
            JobQueueId = execution.JobQueueId,
            UserId = execution.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = parsedStatus,
            StartedAt = execution.StartDateTime,
            CompletedAt = execution.EndDateTime,
            DurationSeconds = execution.EndDateTime.HasValue
                ? (int)(execution.EndDateTime.Value - execution.StartDateTime).TotalSeconds
                : null,
            ErrorMessage = execution.ErrorMessage,
            RetryAttempts = 0
        };
    }

    private async Task<int> EnsureJobMasterAsync(string jobName, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobMaster
            .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (existing != null)
            return existing.JobId;

        var now = DateTime.UtcNow;
        var row = new TblJobMaster
        {
            JobName = jobName,
            Frequency = null,
            Note = "Auto-created by worker runtime",
            TimeToLive = DefaultTimeToLiveSeconds,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.TblJobMaster.Add(row);
        await _context.SaveChangesAsync(cancellationToken);

        return row.JobId;
    }

    private async Task<int> EnsureStatusAsync(int jobId, string status, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobStatus
            .FirstOrDefaultAsync(s => s.JobId == jobId && s.Status == status, cancellationToken);

        if (existing != null)
            return existing.StatusId;

        var row = new TblJobStatus
        {
            JobId = jobId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        _context.TblJobStatus.Add(row);
        await _context.SaveChangesAsync(cancellationToken);

        return row.StatusId;
    }

    private static string BuildStatusNote(JobStatus status) => status switch
    {
        JobStatus.Completed => "Execution completed",
        JobStatus.Failed => "Execution failed",
        JobStatus.Cancelled => "Execution cancelled",
        JobStatus.Skipped => "Execution skipped",
        _ => $"Status changed to {status}"
    };
}
