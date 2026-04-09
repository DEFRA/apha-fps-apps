using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestReqmtRepository : BaseRepository, ITestReqmtRepository
    {
        private readonly IFpsYearContext _fpsYearContext;
        private readonly ICurrentUserContext _currentUserContext;

        public TestReqmtRepository(
            FpsDbContext context,
            IFpsYearContext fpsYearContext,
            ICurrentUserContext currentUserContext) : base(context)
        {
            _fpsYearContext = fpsYearContext;
            _currentUserContext = currentUserContext;
        }

        public async Task<PagedData<TestReqmt>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = _context.TestReqmts
                .AsNoTracking()
                .Where(t => t.TestCode == testCode)
                .AsQueryable();

            baseQuery = ApplyTestReqmtFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.Buyer);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestReqmtDetail>> GetPagedWithDetailsAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = (from t in _context.TestReqmts
                             join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                             join p in _context.Projects on t.Buyer equals p.ParentProject
                             where t.TestCode == testCode
                             select new TestReqmtDetail
                             {
                                 TestCode = t.TestCode,
                                 Buyer = t.Buyer,
                                 UnitPrice = t.UnitPrice,
                                 NoRequired = t.NoRequired,
                                 ProjectBuyerCode = t.ProjectBuyerCode,
                                 TestBuyerCode = t.TestBuyerCode,
                                 DateCreated = t.DateCreated,
                                 Active = t.Active,
                                 FpsYear = t.FpsYear,
                                 IsDefraProject = p.IsDefraProject,
                                 RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                             }).AsQueryable();

            baseQuery = ApplyTestReqmtDetailFilter(baseQuery, query.Filter);

            baseQuery = (!string.IsNullOrWhiteSpace(query.SortBy), query.Descending) switch
            {
                (true, true) => query.SortBy switch
                {
                    nameof(TestReqmtDetail.Buyer) => baseQuery.OrderByDescending(t => t.Buyer),
                    nameof(TestReqmtDetail.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                    nameof(TestReqmtDetail.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                    nameof(TestReqmtDetail.Active) => baseQuery.OrderByDescending(t => t.Active),
                    nameof(TestReqmtDetail.ProjectBuyerCode) => baseQuery.OrderByDescending(t => t.ProjectBuyerCode),
                    nameof(TestReqmtDetail.IsDefraProject) => baseQuery.OrderByDescending(t => t.IsDefraProject),
                    nameof(TestReqmtDetail.RecUnitPrice) => baseQuery.OrderByDescending(t => t.RecUnitPrice),
                    _ => baseQuery.OrderByDescending(t => t.TestCode)
                },
                (true, false) => query.SortBy switch
                {
                    nameof(TestReqmtDetail.Buyer) => baseQuery.OrderBy(t => t.Buyer),
                    nameof(TestReqmtDetail.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                    nameof(TestReqmtDetail.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                    nameof(TestReqmtDetail.Active) => baseQuery.OrderBy(t => t.Active),
                    nameof(TestReqmtDetail.ProjectBuyerCode) => baseQuery.OrderBy(t => t.ProjectBuyerCode),
                    nameof(TestReqmtDetail.IsDefraProject) => baseQuery.OrderBy(t => t.IsDefraProject),
                    nameof(TestReqmtDetail.RecUnitPrice) => baseQuery.OrderBy(t => t.RecUnitPrice),
                    _ => baseQuery.OrderBy(t => t.TestCode)
                },
                _ => baseQuery.OrderBy(t => t.TestCode)
            };

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<TestReqmtDetail>> GetAllForExportAsync(string testCode, string? filterJson)
        {
            var query = (from t in _context.TestReqmts
                         join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                         join p in _context.Projects on t.Buyer equals p.ParentProject
                         where t.TestCode == testCode
                         select new TestReqmtDetail
                         {
                             TestCode = t.TestCode,
                             Buyer = t.Buyer,
                             UnitPrice = t.UnitPrice,
                             NoRequired = t.NoRequired,
                             ProjectBuyerCode = t.ProjectBuyerCode,
                             TestBuyerCode = t.TestBuyerCode,
                             DateCreated = t.DateCreated,
                             Active = t.Active,
                             FpsYear = t.FpsYear,
                             IsDefraProject = p.IsDefraProject,
                             RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                         }).AsQueryable();

            query = ApplyTestReqmtDetailFilter(query, filterJson);

            return await query.OrderBy(t => t.Buyer).ToListAsync();
        }

        public async Task<TestReqmt?> GetByIdAsync(string testCode, string buyer)
        {
            return await _context.TestReqmts
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode == testCode && t.Buyer == buyer);
        }

        public async Task<TestReqmtDetail?> GetDetailByIdAsync(string testCode, string buyer)
        {
            return await (from t in _context.TestReqmts
                          join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                          join p in _context.Projects on t.Buyer equals p.ParentProject
                          where t.TestCode == testCode && t.Buyer == buyer
                          select new TestReqmtDetail
                          {
                              TestCode = t.TestCode,
                              Buyer = t.Buyer,
                              UnitPrice = t.UnitPrice,
                              NoRequired = t.NoRequired,
                              ProjectBuyerCode = t.ProjectBuyerCode,
                              TestBuyerCode = t.TestBuyerCode,
                              DateCreated = t.DateCreated,
                              Active = t.Active,
                              FpsYear = t.FpsYear,
                              IsDefraProject = p.IsDefraProject,
                              RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                          }).FirstOrDefaultAsync();
        }

        public async Task<TestReqmtDetail?> GetPricingAsync(string testCode, string? projectCode)
        {
            var tp = await _context.TestorProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemCode == testCode);

            if (tp is null) return null;

            // TestCode only — return DefraUnitPrice with no project context
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return new TestReqmtDetail
                {
                    TestCode = testCode,
                    RecUnitPrice = tp.DefraUnitPrice
                };
            }

            // TestCode + ProjectCode — apply IsDefraProject logic
            var p = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ParentProject == projectCode);

            if (p is null) return null;

            return new TestReqmtDetail
            {
                TestCode = testCode,
                Buyer = projectCode,
                IsDefraProject = p.IsDefraProject,
                RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
            };
        }

        public async Task<bool> ExistsByTestBuyerCodeAsync(string testBuyerCode)
        {
            return await _context.TestReqmts
                .AsNoTracking()
                .AnyAsync(r => r.TestBuyerCode == testBuyerCode);
        }

        public async Task<bool> ExistsByTestCodeAndBuyerInMonthlyOutputAsync(string testCode, string buyer)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.Buyer == buyer);
        }

        public async Task<TestReqmt> AddAsync(TestReqmt entity)
        {
            entity.FpsYear = _fpsYearContext.FPSYear;
            entity.DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.TestReqmts.AddAsync(entity);
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "I");
            return entity;
        }

        public async Task<TestReqmt> UpdateAsync(TestReqmt entity)
        {
            entity.FpsYear = _fpsYearContext.FPSYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "U");
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer)
        {
            var entity = await _context.TestReqmts
                .FirstOrDefaultAsync(t =>
                    t.TestCode == testCode &&
                    t.Buyer == buyer &&
                    t.FpsYear == _fpsYearContext.FPSYear);

            if (entity is null) return false;

            await WriteAuditLogAsync(entity, "D");
            _context.TestReqmts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<TestReqmt> ApplyTestReqmtFilter(
            IQueryable<TestReqmt> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("Buyer", out string? buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => t.Buyer.Contains(buyer));

            if (filters.TryGetValue("ProjectBuyerCode", out string? projectCode) && !string.IsNullOrWhiteSpace(projectCode))
                query = query.Where(t => t.ProjectBuyerCode != null && t.ProjectBuyerCode.Contains(projectCode));

            return query;
        }

        private static IQueryable<TestReqmtDetail> ApplyTestReqmtDetailFilter(
            IQueryable<TestReqmtDetail> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("Buyer", out string? buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => t.Buyer.Contains(buyer));

            if (filters.TryGetValue("ProjectBuyerCode", out string? projectCode) && !string.IsNullOrWhiteSpace(projectCode))
                query = query.Where(t => t.ProjectBuyerCode != null && t.ProjectBuyerCode.Contains(projectCode));

            return query;
        }

        // ── UITrig: INSERT/UPDATE → 'I'  |  DTrig: DELETE → 'D' ─────────────
        private async Task WriteAuditLogAsync(TestReqmt entity, string insertDelete)
        {
            var log = new TestReqLog
            {
                TestCode      = entity.TestCode,
                Buyer         = entity.Buyer,
                UnitPrice     = entity.UnitPrice.HasValue ? (double?)decimal.ToDouble(entity.UnitPrice.Value) : null,
                NoRequired    = entity.NoRequired.HasValue ? (int?)Convert.ToInt32(entity.NoRequired.Value) : null,
                DateTime      = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId        = _currentUserContext.UserId,
                InsertDelete  = insertDelete,
                FpsYear       = _fpsYearContext.FPSYear
            };

            // UITrig also captures ProjectBuyerCode, TestBuyerCode and Active
            if (insertDelete == "I")
            {
                log.ProjectBuyerCode = entity.ProjectBuyerCode;
                log.TestBuyerCode    = entity.TestBuyerCode;
                log.Active           = entity.Active;
            }

            await _context.TestReqLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
