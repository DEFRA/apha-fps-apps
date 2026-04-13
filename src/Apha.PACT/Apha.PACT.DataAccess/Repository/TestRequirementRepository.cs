using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestRequirementRepository : BaseRepository, ITestRequirementRepository
    {
        private readonly IFpsYearContext _fpsYearContext;
        private readonly ICurrentUserContext _currentUserContext;

        public TestRequirementRepository(
            FpsDbContext context,
            IFpsYearContext fpsYearContext,
            ICurrentUserContext currentUserContext) : base(context)
        {
            _fpsYearContext = fpsYearContext;
            _currentUserContext = currentUserContext;
        }

        public async Task<PagedData<TestRequirement>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = _context.TestRequirements
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

        public async Task<PagedData<TestRequirementDetail>> GetPagedWithDetailsAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = (from t in _context.TestRequirements
                             join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                             join p in _context.Projects on t.Buyer equals p.ParentProject
                             where t.TestCode == testCode
                             select new TestRequirementDetail
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
                    nameof(TestRequirementDetail.Buyer) => baseQuery.OrderByDescending(t => t.Buyer),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderByDescending(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderByDescending(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderByDescending(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderByDescending(t => t.RecUnitPrice),
                    _ => baseQuery.OrderByDescending(t => t.TestCode)
                },
                (true, false) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.Buyer) => baseQuery.OrderBy(t => t.Buyer),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderBy(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderBy(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderBy(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderBy(t => t.RecUnitPrice),
                    _ => baseQuery.OrderBy(t => t.TestCode)
                },
                _ => baseQuery.OrderBy(t => t.TestCode)
            };

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestRequirementDetail>> GetPagedByProjectAsync(
            PaginationParameters<string> query, string parentProject)
        {
            var baseQuery = (from t in _context.TestRequirements
                             join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                             join p in _context.Projects on t.Buyer equals p.ParentProject
                             where t.Buyer == parentProject
                             select new TestRequirementDetail
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
                    nameof(TestRequirementDetail.TestCode) => baseQuery.OrderByDescending(t => t.TestCode),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderByDescending(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderByDescending(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderByDescending(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderByDescending(t => t.RecUnitPrice),
                    _ => baseQuery.OrderByDescending(t => t.TestCode)
                },
                (true, false) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.TestCode) => baseQuery.OrderBy(t => t.TestCode),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderBy(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderBy(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderBy(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderBy(t => t.RecUnitPrice),
                    _ => baseQuery.OrderBy(t => t.TestCode)
                },
                _ => baseQuery.OrderBy(t => t.TestCode)
            };

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<TestRequirementDetail>> GetAllForExportAsync(string testCode, string? filterJson)
        {
            var query = (from t in _context.TestRequirements
                         join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                         join p in _context.Projects on t.Buyer equals p.ParentProject
                         where t.TestCode == testCode
                         select new TestRequirementDetail
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

        public async Task<TestRequirement?> GetByIdAsync(string testCode, string buyer)
        {
            return await _context.TestRequirements
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode == testCode && t.Buyer == buyer);
        }

        public async Task<TestRequirementDetail?> GetDetailByIdAsync(string testCode, string buyer)
        {
            return await (from t in _context.TestRequirements
                          join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                          join p in _context.Projects on t.Buyer equals p.ParentProject
                          where t.TestCode == testCode && t.Buyer == buyer
                          select new TestRequirementDetail
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

        public async Task<TestRequirementDetail?> GetPricingAsync(string testCode, string? projectCode)
        {
            var tp = await _context.TestorProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemCode == testCode);

            if (tp is null) return null;

            // TestCode only — return DefraUnitPrice with no project context
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return new TestRequirementDetail
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

            return new TestRequirementDetail
            {
                TestCode = testCode,
                Buyer = projectCode,
                IsDefraProject = p.IsDefraProject,
                RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
            };
        }

        public async Task<bool> ExistsByTestBuyerCodeAsync(string testBuyerCode)
        {
            return await _context.TestRequirements
                .AsNoTracking()
                .AnyAsync(r => r.TestBuyerCode == testBuyerCode);
        }

        public async Task<bool> ExistsByTestCodeAndBuyerInMonthlyOutputAsync(string testCode, string buyer)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.Buyer == buyer);
        }

        public async Task<TestRequirement> AddAsync(TestRequirement entity)
        {
            entity.FpsYear = _fpsYearContext.FPSYear;
            entity.DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.TestRequirements.AddAsync(entity);
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "I");
            return entity;
        }

        public async Task<TestRequirement> UpdateAsync(TestRequirement entity)
        {
            entity.FpsYear = _fpsYearContext.FPSYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "U");
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer)
        {
            var entity = await _context.TestRequirements
                .FirstOrDefaultAsync(t =>
                    t.TestCode == testCode &&
                    t.Buyer == buyer &&
                    t.FpsYear == _fpsYearContext.FPSYear);

            if (entity is null) return false;

            await WriteAuditLogAsync(entity, "D");
            _context.TestRequirements.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<TestRequirement> ApplyTestReqmtFilter(
            IQueryable<TestRequirement> query, string? filterJson)
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

        private static IQueryable<TestRequirementDetail> ApplyTestReqmtDetailFilter(
            IQueryable<TestRequirementDetail> query, string? filterJson)
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
        private async Task WriteAuditLogAsync(TestRequirement entity, string insertDelete)
        {
            var log = new TestRequirementLog
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

            await _context.TestRequirementLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
