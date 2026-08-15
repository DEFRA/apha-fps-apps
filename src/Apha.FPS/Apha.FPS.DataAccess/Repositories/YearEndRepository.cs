using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class YearEndRepository : BaseRepository, IYearEndRepository
    {
        private readonly IFpsRequestContext _requestContext;
        public YearEndRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext;
        }

        public async Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName)
        {
            IQueryable<BatchJobHistory> jobHistoriesQuery =
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower()
                select new BatchJobHistory
                {
                    JobId = jm.JobId,
                    JobName = jm.JobName,
                    JobExecutionId = jq.JobExecutionId,
                    RequestedBy = jq.RequestedBy,
                    StartDateTime = jq.StartDateTime,
                    EndDateTime = jq.EndDateTime,
                    ErrorMessage = jq.ErrorMessage,
                    Status = js.Status
                };

            jobHistoriesQuery = (IQueryable<BatchJobHistory>)ApplySorting(jobHistoriesQuery, query.SortBy?.ToLower(), query.Descending);
            var result = await jobHistoriesQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName)
        {
            bool hasNonTerminalRecord = await CanInitiateRequest(jobName);

            return !hasNonTerminalRecord;
        }

        public async Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName)
        {
            bool hasRunningJob = await CanApproveOrRejectRequest(jobName);

            return hasRunningJob;
        }

        public async Task<string> GetYearEndDataSetupRequestInitiatorAsync(string jobName)
        {
            return await GetInitiator(jobName);
        }

        public async Task<BatchJobQueue> EnqueueDataSetupInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueInitiationRequest(jobName, requestedBy, correlationId, note);
        }

        public async Task<BatchJobQueue> EnqueueDataSetupApprovalBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueApprovalOrRejectRequest(jobName, requestedBy, correlationId, note, false);
        }

        public async Task<BatchJobQueue> EnqueueDataSetupRejectBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueApprovalOrRejectRequest(jobName, requestedBy, correlationId, note, true);
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName)
        {
            bool hasNonTerminalRecord = await CanInitiateRequest(jobName);

            return !hasNonTerminalRecord;
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<bool> CanApproveOrRejectYearEndCutOverRequestAsync(string jobName)
        {
            bool hasRunningJob = await CanApproveOrRejectRequest(jobName);

            return hasRunningJob;
        }

        [SuppressMessage("SonarAnalyzer.CSharp", "S4144:MethodsShouldNotHaveIdenticalImplementations",
            Justification = "Intentionally identical: DataSetup and CutOver are distinct domain operations that must remain independently named for clarity and maintainability.")]
        public async Task<string> GetYearEndCutOverRequestInitiatorAsync(string jobName)
        {
            return await GetInitiator(jobName);
        }

        public async Task<BatchJobQueue> EnqueueCutOverInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueInitiationRequest(jobName, requestedBy, correlationId, note);
        }

        public async Task<BatchJobQueue> EnqueueCutOverApprovalBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueApprovalOrRejectRequest(jobName, requestedBy, correlationId, note, false);
        }

        public async Task<BatchJobQueue> EnqueueCutOverRejectBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
        {
            return await EnqueueApprovalOrRejectRequest(jobName, requestedBy, correlationId, note, true);
        }

        public async Task SetTriggeredMetadataAsync(string jobExecutionId, string triggeredBy)
        {
            if (string.IsNullOrWhiteSpace(jobExecutionId) || !Guid.TryParse(jobExecutionId, out var parsedJobExecutionId))
            {
                throw new ArgumentException("A valid jobExecutionId is required.", nameof(jobExecutionId));
            }

            // triggered_at_utc is "timestamp without time zone" (see BatchJobQueueMap) - write
            // Kind=Unspecified UTC wall-clock digits, same reasoning as EnqueueApprovalOrRejectRequest.
            // updated_at is genuinely timestamptz, so it keeps Kind=Utc.
            var nowUtc = DateTime.UtcNow;
            var nowUtcNaive = DateTime.SpecifyKind(nowUtc, DateTimeKind.Unspecified);

            var updatedRows = await _context.BatchJobQueues
                .Where(q => q.JobExecutionId == parsedJobExecutionId)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(q => q.TriggeredBy, _ => triggeredBy)
                    .SetProperty(q => q.TriggeredAtUtc, _ => nowUtcNaive)
                    .SetProperty(q => q.UpdatedAt, _ => nowUtc));

            if (updatedRows == 0)
            {
                throw new KeyNotFoundException($"Batch job queue row for JobExecutionId '{jobExecutionId}' was not found.");
            }
        }

        private async Task<bool> CanInitiateRequest(string jobName)
        {
            // Returns true when no records exist for the job, OR every record is in a terminal status (rejected / failed / cancelled).
            // Returns false when at least one record exists that is NOT in a terminal status.
            return await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower()
                   && js.Status.ToLower() != "rejected"
                   && js.Status.ToLower() != "failed"
                   && js.Status.ToLower() != "cancelled"
                select jq.JobqueueId
            ).AnyAsync();
        }

        private async Task<bool> CanApproveOrRejectRequest(string jobName)
        {
            return await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select jq.JobqueueId
            ).AnyAsync();
        }

        private async Task<string> GetInitiator(string jobName)
        {
            var initiator = await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select jq.RequestedBy
            ).FirstOrDefaultAsync();

            return initiator ?? string.Empty;
        }

        private async Task<BatchJobQueue> EnqueueApprovalOrRejectRequest(string jobName, string requestedBy, string correlationId, string note, bool isReject)
        {
            BatchJobQueue queueRow = null!;
            BatchJobStatus jobStatus;

            var jobqueue = await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select new { jq.JobqueueId, jq.JobId }
            ).FirstOrDefaultAsync();

            if (jobqueue == null)
            {
                throw new KeyNotFoundException($"No initiated request was found for job '{jobName}'.");
            }

            if (isReject)
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "rejected")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'rejected' not found for job '{jobName}'.");
            }
            else
            {
                jobStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .Where(s => s.JobId == jobqueue.JobId && s.Status.ToLower() == "approved")
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException($"Status 'approved' not found for job '{jobName}'.");
            }


            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    queueRow = await _context.BatchJobQueues
                    .AsNoTracking().FirstOrDefaultAsync(j => j.JobqueueId == jobqueue.JobqueueId)
                    ?? throw new KeyNotFoundException($"Batch job queue for job '{jobName}' was not found.");

                    //update the status of the job queue entry to "approved" or "rejected"
                    var decidedAtUtc = DateTime.UtcNow;
                    queueRow.StatusId = jobStatus.StatusId;
                    queueRow.RequestedBy = requestedBy;
                    queueRow.RequestedAtUtc = decidedAtUtc;
                    queueRow.StartDateTime = decidedAtUtc;
                    queueRow.ErrorMessage = note;

                    // approved_at_utc/rejected_at_utc are "timestamp without time zone" columns
                    // (see BatchJobQueueMap), unlike requested_at_utc/startdatetime above - Npgsql
                    // requires Kind=Unspecified for a naive-typed parameter, and reinterpreting
                    // (not reconverting) the same instant's wall-clock digits keeps this write
                    // exactly aligned with requestedAtUtc/startDateTime above, just without the PG
                    // offset marker.
                    var decidedAtUtcNaive = DateTime.SpecifyKind(decidedAtUtc, DateTimeKind.Unspecified);

                    // Approval/rejection metadata written in this same transaction as the status
                    // flip - previously missing entirely (see
                    // fps-year-end-phase6-implementation-trace-2026-08-15.md, "Urgent finding").
                    // Apha.BatchJobs.JobOrchestrator.ValidateApprovalMetadataAsync requires
                    // approved_by/approved_at_utc to be populated for a real Approved pickup to
                    // succeed. triggered_by/triggered_at_utc are deliberately NOT set here - they
                    // depend on the outcome of the EventBridge publish call, which happens after
                    // this transaction commits (see EnqueueYearEndDataSetupApprovalJobAsync /
                    // EnqueueYearEndCutOverApprovalJobAsync in YearEndService).
                    if (isReject)
                    {
                        queueRow.RejectedBy = requestedBy;
                        queueRow.RejectedAtUtc = decidedAtUtcNaive;
                        queueRow.RejectionReason = note;
                    }
                    else
                    {
                        queueRow.ApprovedBy = requestedBy;
                        queueRow.ApprovedAtUtc = decidedAtUtcNaive;
                    }

                    _context.BatchJobQueues.Update(queueRow);

                    BatchJobQueueLog logEntry = BuildJobQueueLogEntry(requestedBy, jobqueue.JobqueueId, note, DateTime.UtcNow, jobStatus.StatusId);
                    _context.BatchJobQueueLogs.Add(logEntry);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return queueRow;
        }

        private async Task<BatchJobQueue> EnqueueInitiationRequest(string jobName, string requestedBy, string correlationId, string note)
        {
            BatchJobQueue jobQueueEntry = null!;

            var job = await _context.BatchJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobName.ToLower() == jobName.ToLower())
                ?? throw new KeyNotFoundException($"Batch job '{jobName}' was not found.");

            var initiatedStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.JobId == job.JobId && s.Status.ToLower() == "initiated")
                ?? throw new KeyNotFoundException($"Status 'initiated' not found for job '{jobName}'.");

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    jobQueueEntry = BuildJobQueueEntry(requestedBy, correlationId, note, job.JobId, initiatedStatus.StatusId, _requestContext.FpsYear);
                    _context.BatchJobQueues.Add(jobQueueEntry);

                    BatchJobQueueLog logEntry = BuildJobQueueLogEntry(jobQueueEntry.RequestedBy, jobQueueEntry.JobqueueId, note, jobQueueEntry.StartDateTime, initiatedStatus.StatusId);
                    _context.BatchJobQueueLogs.Add(logEntry);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return jobQueueEntry;
        }

        private static IQueryable ApplySorting(IQueryable<BatchJobHistory> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderByDescending(e => e.StartDateTime);
            }
            else
            {
                return sortBy switch
                {
                    "jobid" => ApplyOrder(query, i => i.JobId, descending),
                    "jobname" => ApplyOrder(query, i => i.JobName, descending),
                    "jobexecutionid" => ApplyOrder(query, i => i.JobExecutionId, descending),
                    "requestedby" => ApplyOrder(query, i => i.RequestedBy, descending),
                    "startdatetime" => ApplyOrder(query, i => i.StartDateTime, descending),
                    "enddatetime" => ApplyOrder(query, i => i.EndDateTime, descending),
                    "errormessage" => ApplyOrder(query, i => i.ErrorMessage, descending),
                    "status" => ApplyOrder(query, i => i.Status, descending),
                    _ => query.OrderBy(e => e.StartDateTime)
                };
            }
        }

        private static IQueryable ApplyOrder<T>(IQueryable<BatchJobHistory> query, Expression<Func<BatchJobHistory, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static BatchJobQueue BuildJobQueueEntry(string requestedBy, string correlationId, string note, int jobId, int statusId, int contextYear)
        {
            return new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(),
                JobExecutionId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid() : Guid.Parse(correlationId),
                JobId = jobId,
                StatusId = statusId,
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                StartDateTime = DateTime.UtcNow,
                ErrorMessage = note,
                FpsYear = contextYear
            };
        }

        private static BatchJobQueueLog BuildJobQueueLogEntry(string requestedBy, Guid jobqueueId, string note, DateTime logtime, int statusId)
        {
            return new BatchJobQueueLog
            {
                JobqueueId = jobqueueId,
                StatusId = statusId,
                PerformedBy = requestedBy,
                LogTime = logtime,
                Note = note
            };
        }
    }
}
