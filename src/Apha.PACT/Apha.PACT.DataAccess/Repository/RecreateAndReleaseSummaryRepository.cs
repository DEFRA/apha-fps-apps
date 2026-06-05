using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class RecreateAndReleaseSummaryRepository : BaseRepository, IRecreateAndReleaseSummaryRepository
    {
        public RecreateAndReleaseSummaryRepository(FpsDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves recreate summaries logs with pagination and sorting.
        /// Implements the SQL query logic: LEFT JOIN tblUsers ON CONCAT('CVLNT', UserID) = UserName
        /// Returns enriched log data with user information.
        /// </summary>
        public async Task<PagedData<RecreateSummaryLogWithComment>> GetRecreateSummaryLogAsync(PaginationParameters<string> parameters)
        {
            // Build the query with LEFT JOIN - EF Core will translate string concatenation to SQL CONCAT
            var baseQuery = from log in _context.RecreateSummaryLogs.AsNoTracking()
                            join user in _context.Users.AsNoTracking()
                            on ("CVLNT" + log.UserId) equals user.UserName into userGroup
                            from user in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Log = log,
                                UserName = user != null ? user.UserName : null,
                                UserComments = user != null ? user.Comments : null
                            };

            // Apply sorting at database level
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                var sortBy = parameters.SortBy.ToLower();
                baseQuery = (sortBy, parameters.Descending) switch
                {
                    ("id", true) => baseQuery.OrderByDescending(e => e.Log.Id),
                    ("id", false) => baseQuery.OrderBy(e => e.Log.Id),
                    ("datedone", true) => baseQuery.OrderByDescending(e => e.Log.DateDone),
                    ("datedone", false) => baseQuery.OrderBy(e => e.Log.DateDone),
                    ("userid", true) => baseQuery.OrderByDescending(e => e.Log.UserId),
                    ("userid", false) => baseQuery.OrderBy(e => e.Log.UserId),
                    ("user", true) => baseQuery.OrderByDescending(e => e.UserComments),
                    ("user", false) => baseQuery.OrderBy(e => e.UserComments),
                    ("period", true) => baseQuery.OrderByDescending(e => e.Log.Period),
                    ("period", false) => baseQuery.OrderBy(e => e.Log.Period),
                    _ => baseQuery.OrderByDescending(e => e.Log.DateDone)
                };
            }
            else
            {
                baseQuery = baseQuery.OrderByDescending(e => e.Log.DateDone);
            }

            // Apply pagination at database level before materialization
            var pagedQuery = baseQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);

            // Materialize only the paginated results
            var result = await pagedQuery.ToListAsync();

            // Map to the result model
            var mappedData = result.Select(r => new RecreateSummaryLogWithComment
            {
                Id = r.Log.Id,
                UserId = r.Log.UserId,
                Comments = r.UserComments ?? string.Empty,
                Period = r.Log.Period,
                DateDone = r.Log.DateDone,
            }).ToList();

            return base.ApplyPaging(mappedData, parameters.Page, parameters.PageSize);
        }
    }
}