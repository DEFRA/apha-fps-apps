using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestSupplierRepository : BaseRepository, ITestSupplierRepository
    {
        public TestSupplierRepository(FpsDbContext context) : base(context) { }

        public async Task<PagedData<TestSupplierView>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode, bool showRejected)
        {
            // TestCost (money * int) cannot be computed in SQL on PostgreSQL due to
            // money-type casting restrictions; it is calculated client-side after fetch.
            var baseQuery = (from tr in _context.TestRequirements
                             join p in _context.Projects on tr.Buyer equals p.ParentProject
                             where tr.TestCode == testCode
                             && (showRejected || tr.Active == 1)
                             select new TestSupplierView
                             {
                                 TestCode = tr.TestCode,
                                 Buyer = tr.Buyer,
                                 ProjectManager = p.Manager,
                                 NoRequired = tr.NoRequired,
                                 UnitPrice = tr.UnitPrice,
                                 TestCost = null,
                                 ProjectStatus = p.ProjectStatus
                             }).AsQueryable();

            baseQuery = ApplyFilter(baseQuery, query.Filter);

            // TestCost sort is deferred to client-side below; all other sorts run in DB.
            bool sortByTestCost = string.Equals(query.SortBy, nameof(TestSupplierView.TestCost),
                StringComparison.Ordinal);

            if (!sortByTestCost)
            {
                baseQuery = (!string.IsNullOrWhiteSpace(query.SortBy), query.Descending) switch
                {
                    (true, true) => query.SortBy switch
                    {
                        nameof(TestSupplierView.Buyer) => baseQuery.OrderByDescending(t => t.Buyer),
                        nameof(TestSupplierView.ProjectManager) => baseQuery.OrderByDescending(t => t.ProjectManager),
                        nameof(TestSupplierView.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                        nameof(TestSupplierView.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                        nameof(TestSupplierView.ProjectStatus) => baseQuery.OrderByDescending(t => t.ProjectStatus),
                        _ => baseQuery.OrderByDescending(t => t.Buyer)
                    },
                    (true, false) => query.SortBy switch
                    {
                        nameof(TestSupplierView.Buyer) => baseQuery.OrderBy(t => t.Buyer),
                        nameof(TestSupplierView.ProjectManager) => baseQuery.OrderBy(t => t.ProjectManager),
                        nameof(TestSupplierView.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                        nameof(TestSupplierView.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                        nameof(TestSupplierView.ProjectStatus) => baseQuery.OrderBy(t => t.ProjectStatus),
                        _ => baseQuery.OrderBy(t => t.Buyer)
                    },
                    _ => baseQuery.OrderBy(t => t.Buyer)
                };
            }

            var rows = await baseQuery.ToListAsync();

            // Compute TestCost client-side to avoid PostgreSQL money-type cast error.
            foreach (var row in rows)
            {
                row.TestCost = row.NoRequired.HasValue && row.UnitPrice.HasValue
                    ? (decimal)row.NoRequired.Value * row.UnitPrice.Value
                    : null;
            }

            // Apply TestCost sort in memory now that the computed value is available.
            IEnumerable<TestSupplierView> result = sortByTestCost
                ? (query.Descending
                    ? rows.OrderByDescending(t => t.TestCost)
                    : rows.OrderBy(t => t.TestCost))
                : rows;

            return ApplyPaging(result.ToList(), query.Page, query.PageSize);
        }

        private static IQueryable<TestSupplierView> ApplyFilter(
            IQueryable<TestSupplierView> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(nameof(TestSupplierView.Buyer), out string? buyer)
                && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => EF.Functions.ILike(t.Buyer, $"%{buyer}%"));

            if (filters.TryGetValue(nameof(TestSupplierView.ProjectStatus), out string? status)
                && !string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.ProjectStatus != null
                    && EF.Functions.ILike(t.ProjectStatus, $"%{status}%"));

            return query;
        }
    }
}
