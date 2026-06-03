using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestSupplierRepository : BaseRepository, ITestSupplierRepository
    {
        private readonly FpsDbContext _dbContext;

        public TestSupplierRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<TestSupplierView>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query,
            string testCode,
            bool showRejected)
        {
            // Step 1 — IQueryable: join TestRequirements + Projects, no arithmetic yet.
            // Both sets have HasQueryFilter(e => e.FpsYear == FilterFpsYear) applied automatically.
            var rawQuery = _dbContext.TestRequirements
                .AsNoTracking()
                .Where(tr => tr.TestCode == testCode)
                .Join(
                    _dbContext.Projects.AsNoTracking(),
                    tr => tr.Buyer,
                    p => p.ParentProject,
                    (tr, p) => new
                    {
                        tr.TestCode,
                        tr.Buyer,
                        p.Manager,
                        tr.NoRequired,
                        tr.UnitPrice,
                        p.ProjectStatus
                    });

            if (!showRejected)
                rawQuery = rawQuery.Where(x => x.ProjectStatus != "rejected");

            rawQuery = ApplySortingRaw(rawQuery, query.SortBy, query.Descending);

            var raw = await rawQuery.ToListAsync();

            // Step 2 — LINQ-to-Objects: safe mixed-type arithmetic (double × decimal).
            var views = raw.Select(x => new TestSupplierView
            {
                TestCode = x.TestCode,
                JobCode = x.Buyer,
                ProjectManager = x.Manager,
                NoTests = x.NoRequired,
                TestPrice = x.UnitPrice,
                TestCost = (decimal)(x.NoRequired ?? 0) * (x.UnitPrice ?? 0),
                ProjectStatus = x.ProjectStatus
            }).ToList();

            return ApplyPaging(views, query.Page, query.PageSize);
        }

        public async Task<TestRequirement?> GetByIdAsync(string testCode, string buyer)
        {
            return await _dbContext.TestRequirements
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.TestCode == testCode && tr.Buyer == buyer);
        }

        public async Task<TestRequirement> AddAsync(TestRequirement entity)
        {
            // Pattern A for UITrig_tlkpTestReqmt (INSERT log) — staged before single SaveChangesAsync.
            _dbContext.TestRequirements.Add(entity);
            _dbContext.TestRequirementLogs.Add(MapEntityToLog(entity, "I"));
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<TestRequirement> UpdateAsync(TestRequirement entity)
        {
            // Pattern A for UITrig_tlkpTestReqmt (UPDATE log) — staged before single SaveChangesAsync.
            _dbContext.TestRequirements.Update(entity);
            _dbContext.TestRequirementLogs.Add(MapEntityToLog(entity, "I"));
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer)
        {
            var entity = await _dbContext.TestRequirements
                .FirstOrDefaultAsync(tr => tr.TestCode == testCode && tr.Buyer == buyer);

            if (entity == null)
                return false;

            // Pattern A for DTrig_tlkpTestReqmt (DELETE log) — staged before single SaveChangesAsync.
            _dbContext.TestRequirementLogs.Add(MapEntityToLog(entity, "D"));
            _dbContext.TestRequirements.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<TestOrProduct>> GetTestOrProductsAsync()
        {
            return await _dbContext.TestOrProducts
                .AsNoTracking()
                .OrderBy(t => t.ItemCode)
                .ToListAsync();
        }

        public async Task<bool> ProjectExistsAsync(string parentProject)
        {
            return await _dbContext.Projects
                .AnyAsync(p => p.ParentProject == parentProject);
        }

        public async Task<bool> TestBuyerCodeExistsAsync(string testCode, string workGroup)
        {
            return await _dbContext.TestCapabilities
                .AnyAsync(tc => tc.TestCode == testCode && tc.WorkGroup == workGroup);
        }

        public async Task<bool> MonthlyOutputExistsAsync(string testCode, string buyer)
        {
            return await _dbContext.MonthlyOutputs
                .AnyAsync(mo => mo.TestCode == testCode && mo.Buyer == buyer);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static TestRequirementLog MapEntityToLog(TestRequirement entity, string insertDelete)
        {
            return new TestRequirementLog
            {
                TestCode = entity.TestCode,
                Buyer = entity.Buyer,
                UnitPrice = entity.UnitPrice,
                NoRequired = entity.NoRequired,
                ProjectBuyerCode = entity.ProjectBuyerCode,
                TestBuyerCode = entity.TestBuyerCode,
                Active = entity.Active,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                InsertDelete = insertDelete,
                FpsYear = entity.FpsYear
            };
        }

        private static IQueryable<T> ApplySortingRaw<T>(
            IQueryable<T> query, string? sortBy, bool descending)
            where T : class
            => query; // Default: no additional ordering (filter applies before paging)

        private static IQueryable<TestSupplierView> ApplySorting(
            IQueryable<TestSupplierView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy)) return query;
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<TestSupplierView> ApplySortingByProperty(
            IQueryable<TestSupplierView> query, string property, bool descending)
        {
            return property switch
            {
                "testcode" => ApplyOrder(query, i => i.TestCode, descending),
                "jobcode" => ApplyOrder(query, i => i.JobCode, descending),
                "projectmanager" => ApplyOrder(query, i => i.ProjectManager, descending),
                "notests" => ApplyOrder(query, i => i.NoTests, descending),
                "testprice" => ApplyOrder(query, i => i.TestPrice, descending),
                "testcost" => ApplyOrder(query, i => i.TestCost, descending),
                "projectstatus" => ApplyOrder(query, i => i.ProjectStatus, descending),
                _ => query
            };
        }

        private static IQueryable<TestSupplierView> ApplyOrder<TKey>(
            IQueryable<TestSupplierView> query,
            Expression<Func<TestSupplierView, TKey>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
