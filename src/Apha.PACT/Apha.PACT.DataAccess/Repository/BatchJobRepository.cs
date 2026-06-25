using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.NetworkInformation;

namespace Apha.PACT.DataAccess.Repository
{
    public class BatchJobRepository : BaseRepository, IBatchJobRepository
    {
        public BatchJobRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName)
        {
            IQueryable<BatchJobHistory> jobHistoriesQuery =
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName == jobName
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

            jobHistoriesQuery = (IQueryable<BatchJobHistory>)ApplySorting(jobHistoriesQuery, query.SortBy, query.Descending);

            return await ApplyPaging(jobHistoriesQuery, query.Page, query.PageSize);
        }

        public async Task<bool> CanRunBatchJobAsync(string jobName)
        {
            var hasRunningJob = await (
                from jm in _context.BatchJobs.AsNoTracking()
                join jq in _context.BatchJobQueues.AsNoTracking() on jm.JobId equals jq.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jm.JobName == jobName && js.Status == "Running"
                select jq.JobqueueId
            ).AnyAsync();

            return !hasRunningJob;
        }

        public async Task<BatchJobQueue> EnqueueBatchJobAsync(string jobName, string requestedBy, string correlationId,string note)
        {
            var job = await _context.BatchJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobName == jobName)
                ?? throw new KeyNotFoundException($"Batch job '{jobName}' was not found.");

            var runningStatus = await _context.BatchJobStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.JobId == job.JobId && s.Status == "Running")
                ?? throw new KeyNotFoundException($"Status 'Running' not found for job '{jobName}'.");

            var entry = new BatchJobQueue
            {
                JobExecutionId = string.IsNullOrEmpty(correlationId)? Guid.NewGuid():Guid.Parse(correlationId),
                JobId = job.JobId,
                StatusId = runningStatus.StatusId,
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                StartDateTime = DateTime.UtcNow,
                ErrorMessage = note
            };

            _context.BatchJobQueues.Add(entry);
            await _context.SaveChangesAsync();

            return entry;
        }

        private static IQueryable ApplySorting(IQueryable<BatchJobHistory> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.StartDateTime);
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
    }
}
