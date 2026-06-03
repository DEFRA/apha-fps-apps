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
        /// Returns User.Comments as the "User" field.
        /// </summary>
        public async Task<PagedData<RecreateSummaryLogs>> GetRecreateSummariesLogsAsync(PaginationParameters<string> parameters)
        {
            // For EF Core In-Memory compatibility, we'll perform the join logic differently
            // Load all logs with users (since In-Memory doesn't support CONCAT in join conditions)
            var logsQuery = _context.RecreateSummaryLogs.AsNoTracking();
            var usersQuery = _context.Users.AsNoTracking();

            // Materialize both sets for in-memory join
            var allLogs = await logsQuery.ToListAsync();
            var allUsers = await usersQuery.ToListAsync();

            // Perform the join in memory with CVLNT concatenation
            var joinedData = from log in allLogs
                             join user in allUsers
                             on "CVLNT" + log.UserId equals user.UserName into userGroup
                             from user in userGroup.DefaultIfEmpty()
                             select new
                             {
                                 Log = log,
                                 UserComments = user?.Comments,
                                 UserName = user?.UserName
                             };

            // Apply sorting
            joinedData = ApplySortingInMemory(joinedData, parameters.SortBy, parameters.Descending);

            // Get total count before pagination
            var totalRecords = joinedData.Count();

            // Apply pagination in memory
            var pagedResults = joinedData
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            // Map the results back to RecreateSummaryLogs entities
            var data = pagedResults.Select(r => new RecreateSummaryLogs
            {
                Id = r.Log.Id,
                UserId = r.Log.UserId,
                Period = r.Log.Period,
                DateDone = r.Log.DateDone,
                FpsYear = r.Log.FpsYear,
                User = (r.UserComments != null && r.UserName != null)
                    ? new User
                    {
                        UserName = r.UserName,
                        Comments = r.UserComments,
                        Logs = new List<RecreateSummaryLogs>()
                    }
                    : new User
                    {
                        UserName = string.Empty,
                        Comments = string.Empty,
                        Logs = new List<RecreateSummaryLogs>()
                    }
            }).ToList();

            // Build pagination metadata
            var paginationData = new PaginationData
            {
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / parameters.PageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<RecreateSummaryLogs>(data.AsReadOnly(), paginationData);
        }

        /// <summary>
        /// Applies sorting to in-memory joined data.
        /// </summary>
        private static IEnumerable<T> ApplySortingInMemory<T>(IEnumerable<T> data, string? sortBy, bool descending)
            where T : class
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "datedone";
                descending = true;
            }

            var property = sortBy.ToLower();

            // Use reflection for in-memory sorting
            return property switch
            {
                "id" => descending
                    ? data.OrderByDescending(x => GetPropertyValue(x, "Log.Id"))
                    : data.OrderBy(x => GetPropertyValue(x, "Log.Id")),

                "datedone" => descending
                    ? data.OrderByDescending(x => GetPropertyValue(x, "Log.DateDone"))
                    : data.OrderBy(x => GetPropertyValue(x, "Log.DateDone")),

                "userid" => descending
                    ? data.OrderByDescending(x => GetPropertyValue(x, "Log.UserId"))
                    : data.OrderBy(x => GetPropertyValue(x, "Log.UserId")),

                "user" => descending
                    ? data.OrderByDescending(x => GetPropertyValue(x, "UserComments"))
                    : data.OrderBy(x => GetPropertyValue(x, "UserComments")),

                "period" => descending
                    ? data.OrderByDescending(x => GetPropertyValue(x, "Log.Period"))
                    : data.OrderBy(x => GetPropertyValue(x, "Log.Period")),

                // Default: invalid field should sort by DateDone descending
                _ => data.OrderByDescending(x => GetPropertyValue(x, "Log.DateDone"))
            };
        }

        private static object? GetPropertyValue(object obj, string propertyPath)
        {
            var current = obj;
            foreach (var prop in propertyPath.Split('.'))
            {
                if (current == null) return null;
                var propInfo = current.GetType().GetProperty(prop);
                current = propInfo?.GetValue(current);
            }
            return current;
        }
    }
}
