using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
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
               where  jm.JobName.ToLower() == jobName.ToLower()
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
            // Returns true when no records exist for the job, OR every record is in a terminal
            // status (rejected / failed / cancelled).
            // Returns false when at least one record exists that is NOT in a terminal status.
            var hasNonTerminalRecord = await (
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

            return !hasNonTerminalRecord;
        }

        public async Task<bool> CanApproveYearEndDataSetupRequestAsync(string jobName)
        {
            var hasRunningJob = await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName.ToLower() == jobName.ToLower() && (js.Status.ToLower() == "initiated")
                select jq.JobqueueId
            ).AnyAsync();

            return hasRunningJob;
        }

        public async Task<BatchJobQueue> EnqueueDataSetupBatchJobAsync(string jobName, string requestedBy, string correlationId,string note)
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

        public async Task<BatchJobQueue> EnqueueApprovedDataSetupBatchJobAsync(string jobName, string requestedBy, string correlationId, string note)
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
