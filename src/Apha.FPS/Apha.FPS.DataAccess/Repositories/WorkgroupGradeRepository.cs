using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for WorkgroupGrade CRUD and lookup data access.
    /// </summary>
    public class WorkgroupGradeRepository : BaseRepository, IWorkgroupGradeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkgroupGradeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<PagedData<WorkgroupGrade>> GetAllWorkgroupGradesPagedAsync(
            PaginationParameters<string> query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = _dbContext.WorkgroupGrades
                .AsNoTracking()
                .AsQueryable();

            baseQuery = ApplyWorkgroupGradeFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<WorkgroupGrade>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync(cancellationToken);
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<WorkgroupGrade?> GetByWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return null;

            return await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.WgGrade == wgGrade, cancellationToken);
        }

        public async Task<WorkgroupGrade> CreateAsync(WorkgroupGrade entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.FpsYear = _requestContext.FpsYear;
            _dbContext.WorkgroupGrades.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<WorkgroupGrade> UpdateAsync(WorkgroupGrade entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(e => e.WgGrade == entity.WgGrade, cancellationToken);

            if (existing is null)
                throw new KeyNotFoundException($"WorkgroupGrade '{entity.WgGrade}' not found.");

            existing.ProfitCentreGrade = entity.ProfitCentreGrade;
            existing.GradeCode = entity.GradeCode;
            existing.Workgroup = entity.Workgroup;
            existing.FpsYear = _requestContext.FpsYear;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            var deleted = await _dbContext.WorkgroupGrades
                .Where(e => e.WgGrade == wgGrade)
                .ExecuteDeleteAsync(cancellationToken);

            return deleted > 0;
        }

        public async Task<bool> HasAssociatedStaffAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            return await _dbContext.WgEmployees
                .AnyAsync(e => e.WorkGroupGrade == wgGrade, cancellationToken);
        }

        public async Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProfitcentreGrades
                .AsNoTracking()
                .Select(e => e.PcGrade)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetAllGradeCodesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Grades
                .AsNoTracking()
                .Select(e => e.GradeCode)
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workgroups
                .AsNoTracking()
                .Select(e => e.WorkgroupName)
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }

        // ─── Sorting ──────────────────────────────────────────────────────────────

        private static IQueryable ApplySorting(IQueryable<WorkgroupGrade> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<WorkgroupGrade> query, string property, bool descending)
        {
            return property switch
            {
                "wggrade" => ApplyOrder(query, e => e.WgGrade, descending),
                "profitcentregrade" => ApplyOrder(query, e => e.ProfitCentreGrade, descending),
                "gradecode" => ApplyOrder(query, e => e.GradeCode, descending),
                "workgroup" => ApplyOrder(query, e => e.Workgroup, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(
            IQueryable<WorkgroupGrade> query,
            Expression<Func<WorkgroupGrade, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        // ─── Filtering ────────────────────────────────────────────────────────────

        private static IQueryable<WorkgroupGrade> ApplyWorkgroupGradeFilter(
            IQueryable<WorkgroupGrade> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WgGrade", out var wgGrade) && wgGrade != null)
                query = query.Where(e => e.WgGrade.Contains(wgGrade.ToString()!));

            if (dict.TryGetValue("ProfitCentreGrade", out var pcGrade) && pcGrade != null)
                query = query.Where(e => e.ProfitCentreGrade.Contains(pcGrade.ToString()!));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(e => e.GradeCode.Contains(gradeCode.ToString()!));

            if (dict.TryGetValue("Workgroup", out var workgroup) && workgroup != null)
                query = query.Where(e => e.Workgroup.Contains(workgroup.ToString()!));

            return query;
        }
    }
}
