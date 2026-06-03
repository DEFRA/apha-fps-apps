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
            // Build query with explicit LEFT JOIN matching the SQL:
            // LEFT JOIN tblUsers u ON CONCAT('CVLNT', rsl.UserID) = u.UserName
            var query = from log in _context.RecreateSummaryLogs.AsNoTracking()
                        join user in _context.Users.AsNoTracking()
                        on "CVLNT" + log.UserId equals user.UserName into userGroup
                        from user in userGroup.DefaultIfEmpty()
                        select new
                        {
                            Log = log,
                            UserComments = user != null ? user.Comments : null,
                            UserName = user != null ? user.UserName : null
                        };

            // Apply sorting based on the SQL query's ORDER BY
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Apply pagination at database level
            var pagedResults = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // Map the results back to RecreateSummaryLogs entities
            // Note: Since we're using a projection, we need to reconstruct the entity with the User navigation property
            var data = pagedResults.Select(r =>
            {
                var log = r.Log;
                // Set the User navigation property if user data exists
                if (r.UserComments != null && r.UserName != null)
                {
                    log.User = new User
                    {
                        UserName = r.UserName,
                        Comments = r.UserComments,
                        Logs = new List<RecreateSummaryLogs>()
                    };
                }
                return log;
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
        /// Applies sorting to the query based on the sortBy parameter.
        /// Matches the SQL query's ORDER BY logic.
        /// Note: "user" field refers to User.Comments (as per SQL: u.Comments AS "User")
        /// </summary>
        private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, bool descending)
            where T : class
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                // Default sort by DateDone descending (matching SQL: ORDER BY rsl.DateDone)
                sortBy = "datedone";
                descending = true;
            }

            var property = sortBy.ToLower();

            // Use dynamic LINQ or reflection to apply sorting
            // Since we're working with an anonymous type, we need to use string-based sorting
            return property switch
            {
                "id" => descending
                    ? query.OrderBy("Log.Id descending")
                    : query.OrderBy("Log.Id"),

                "datedone" => descending
                    ? query.OrderBy("Log.DateDone descending")
                    : query.OrderBy("Log.DateDone"),

                "userid" => descending
                    ? query.OrderBy("Log.UserId descending")
                    : query.OrderBy("Log.UserId"),

                "user" => descending
                    ? query.OrderBy("UserComments descending")
                    : query.OrderBy("UserComments"),

                "period" => descending
                    ? query.OrderBy("Log.Period descending")
                    : query.OrderBy("Log.Period"),

                _ => descending
                    ? query.OrderBy("Log.DateDone descending")
                    : query.OrderBy("Log.DateDone")
            };
        }
    }

    /// <summary>
    /// Extension method for dynamic sorting with string property names
    /// </summary>
    internal static class QueryableExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return source;

            var descending = propertyName.EndsWith(" descending", StringComparison.OrdinalIgnoreCase);
            if (descending)
                propertyName = propertyName.Substring(0, propertyName.Length - " descending".Length).Trim();

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            System.Linq.Expressions.Expression property = parameter;

            foreach (var member in propertyName.Split('.'))
            {
                property = System.Linq.Expressions.Expression.PropertyOrField(property, member);
            }

            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);
            var methodName = descending ? "OrderByDescending" : "OrderBy";

            var resultExpression = System.Linq.Expressions.Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                source.Expression,
                System.Linq.Expressions.Expression.Quote(lambda));

            return source.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
