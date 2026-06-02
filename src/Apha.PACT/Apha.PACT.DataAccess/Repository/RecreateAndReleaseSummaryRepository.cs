using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class RecreateAndReleaseSummaryRepository : BaseRepository, IRecreateAndReleaseSummaryRepository
    {
        public RecreateAndReleaseSummaryRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<RecreateSummariesLog>> GetRecreateSummariesAllLogsAsync(PaginationParameters<string> parameters)
        {
            IQueryable<RecreateSummariesLog> query = _context.RecreateSummariesLogs
                .Include(r => r.User)
                .AsNoTracking();

            // Apply sorting
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Apply pagination at database level
            var data = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // Build pagination metadata
            var paginationData = new PaginationData
            {
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / parameters.PageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<RecreateSummariesLog>(data.AsReadOnly(), paginationData);
        }

        private static IQueryable<RecreateSummariesLog> ApplySorting(IQueryable<RecreateSummariesLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderByDescending(e => e.DateDone);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<RecreateSummariesLog> ApplySortingByProperty(IQueryable<RecreateSummariesLog> query, string property, bool descending)
        {
            return property switch
            {
                "id" => ApplyOrder(query, r => r.Id, descending),
                "datedone" => ApplyOrder(query, r => r.DateDone, descending),
                "userid" => ApplyOrder(query, r => r.UserId, descending),
                "user" => ApplyOrder(query, r => r.User!.UserName, descending),
                "period" => ApplyOrder(query, r => r.Period, descending),
                _ => query.OrderByDescending(e => e.DateDone)
            };
        }

        private static IQueryable<RecreateSummariesLog> ApplyOrder<T>(IQueryable<RecreateSummariesLog> query, Expression<Func<RecreateSummariesLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
