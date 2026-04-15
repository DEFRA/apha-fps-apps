using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// Implementation of job execution repository using EF Core.
/// </summary>
public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly BatchJobsDbContext _context;
    private const int DefaultTimeToLiveSeconds = 3600;
    private const string SystemActor = "BatchWorker";

    /// <summary>
    /// Initializes a new instance of the JobExecutionRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    public JobExecutionRepository(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var runGuid = ParseRunId(record.RunId);
        var now = DateTime.UtcNow;
        var jobId = await EnsureJobMasterAsync(record.JobName, cancellationToken);
        var statusId = await EnsureStatusAsync(jobId, record.Status.ToString(), cancellationToken);

        var queueRow = new TblJobQueue
        {
            JobQueueId = runGuid,
            JobId = jobId,
            StatusId = statusId,
            StartDateTime = record.StartedAt,
            EndDateTime = null,
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.TblJobQueue.Add(queueRow);
        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = runGuid,
            StatusId = statusId,
            PerformedBy = SystemActor,
            LogTime = now,
            Note = "Execution started"
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Foundation queue uses GUID correlation ID. The int return is retained
        // for interface compatibility with orchestrator and tests.
        return 0;
    }

    /// <inheritdoc />
    public async Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var runGuid = ParseRunId(record.RunId);
        var queueRow = await _context.TblJobQueue
            .FirstOrDefaultAsync(q => q.JobQueueId == runGuid, cancellationToken);

        if (queueRow == null)
            return;

        var now = DateTime.UtcNow;
        var statusId = await EnsureStatusAsync(queueRow.JobId, record.Status.ToString(), cancellationToken);

        queueRow.StatusId = statusId;
        queueRow.EndDateTime = record.CompletedAt;
        queueRow.ErrorMessage = record.ErrorMessage;
        queueRow.UpdatedAt = now;

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = runGuid,
            StatusId = statusId,
            PerformedBy = SystemActor,
            LogTime = now,
            Note = BuildStatusNote(record.Status)
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetExecutionRecordAsync(int executionId, CancellationToken cancellationToken = default)
    {
        // Foundation schema identifies runs by GUID (jobqueueid), not int.
        // Keep method for backward compatibility with the interface.
        return await Task.FromResult<JobExecutionRecord?>(null);
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
                q.JobQueueId,
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
            RunId = last.JobQueueId.ToString("N"),
            JobType = JobType.Unknown,
            RunMode = RunMode.AdHoc,
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

    private static Guid ParseRunId(string runId)
    {
        if (Guid.TryParseExact(runId, "N", out var guidN))
            return guidN;

        if (Guid.TryParse(runId, out var guid))
            return guid;

        throw new ArgumentException("RunId must be a valid GUID string.", nameof(runId));
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
