using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// LINQ-first repository for Workgroup CRUD, paged queries, and lookup data.
    /// All queries are automatically scoped to the active FPS year via the DbContext
    /// HasQueryFilter registered in FpsDbContext.OnModelCreating.
    /// </summary>
    public class WorkgroupRepository : BaseRepository, IWorkgroupRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkgroupRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
            : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        /// <inheritdoc/>
        public async Task<PagedData<Workgroup>> GetPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = _dbContext.Workgroups
                .AsNoTracking()
                .AsQueryable();

            baseQuery = ApplyWorkgroupFilter(baseQuery, query.Filter);

            baseQuery = ApplyWorkgroupSorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        /// <inheritdoc/>
        public async Task<Workgroup?> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return null;

            return await _dbContext.Workgroups
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkGroupName == workGroupName);
        }

        /// <inheritdoc/>
        public async Task<Workgroup> CreateAsync(Workgroup workgroup)
        {
            ArgumentNullException.ThrowIfNull(workgroup);

            workgroup.FpsYear = _requestContext.FpsYear;

            _dbContext.Workgroups.Add(workgroup);
            await _dbContext.SaveChangesAsync();
            return workgroup;
        }

        /// <inheritdoc/>
        public async Task<Workgroup> UpdateAsync(string originalWorkGroupName, Workgroup workgroup)
        {
            ArgumentNullException.ThrowIfNull(workgroup);
            if (string.IsNullOrWhiteSpace(originalWorkGroupName))
                throw new ArgumentException("Original WorkGroupName must be supplied.", nameof(originalWorkGroupName));

            var existing = await _dbContext.Workgroups
                .FirstOrDefaultAsync(w => w.WorkGroupName == originalWorkGroupName);

            if (existing is null)
                throw new KeyNotFoundException($"Workgroup '{originalWorkGroupName}' not found for the active FPS year.");

            existing.WorkGroupName    = workgroup.WorkGroupName;
            existing.ProfitCentre     = workgroup.ProfitCentre;
            existing.CostCentre       = workgroup.CostCentre;
            existing.CostCentreOld    = workgroup.CostCentreOld;
            existing.Owner            = workgroup.Owner;
            existing.Description      = workgroup.Description;
            existing.CentralOverhead  = workgroup.CentralOverhead;
            existing.SendEmail        = workgroup.SendEmail;
            existing.Cos90            = workgroup.Cos90;
            existing.EmailRecipient   = workgroup.EmailRecipient;
            existing.FpsYear          = _requestContext.FpsYear;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            var deleted = await _dbContext.Workgroups
                .Where(w => w.WorkGroupName == workGroupName)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            return await _dbContext.Workgroups
                .AsNoTracking()
                .AnyAsync(w => w.WorkGroupName == workGroupName);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAllProfitCentresAsync()
        {
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .Select(pc => pc.ProfitCentreId)
                .Distinct()
                .OrderBy(pc => pc)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Manager>> GetOwnersAsync()
        {
            var result = await _dbContext.StaffActiveView
                .AsNoTracking()
                .Join(
                    _dbContext.WorkgroupGradeGeneralViews.AsNoTracking(),
                    staff  => staff.WorkgroupGrade,
                    wggg   => wggg.WgGrade,
                    (staff, wggg) => new
                    {
                        staff.Name,
                        wggg.WorkGroup,
                        wggg.GradeCode
                    })
                .Where(x => x.Name != null
                         && !x.Name.ToLower().Contains("general")
                         && !x.Name.ToLower().Contains("vacancy"))
                .Where(x => x.GradeCode != null && !x.GradeCode.StartsWith("G"))
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => new Manager
                {
                    Name      = x.Name,
                    WorkGroup = x.WorkGroup,
                    GradeCode = x.GradeCode,
                    Expr1     = x.GradeCode != null
                                    ? x.GradeCode.Substring(0, 1)
                                    : null
                })
                .ToListAsync();

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return Enumerable.Empty<double?>();

            return await _dbContext.Workgroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.CostCentre != null)
                .Select(w => w.CostCentre)
                .Distinct()
                .OrderBy(cc => cc)
                .ToListAsync();
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private static IQueryable<Workgroup> ApplyWorkgroupFilter(
            IQueryable<Workgroup> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WorkGroupName", out var workGroupName) && workGroupName != null)
                query = query.Where(w => w.WorkGroupName.Contains(workGroupName.ToString()!));

            if (dict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(w => w.ProfitCentre.Contains(profitCentre.ToString()!));

            if (dict.TryGetValue("Description", out var description) && description != null)
                query = query.Where(w => w.Description != null && w.Description.Contains(description.ToString()!));

            return query;
        }

        private static IQueryable<Workgroup> ApplyWorkgroupSorting(
            IQueryable<Workgroup> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(w => w.WorkGroupName);

            return sortBy.ToLower() switch
            {
                "workgroupname"   => descending ? query.OrderByDescending(w => w.WorkGroupName)   : query.OrderBy(w => w.WorkGroupName),
                "profitcentre"    => descending ? query.OrderByDescending(w => w.ProfitCentre)    : query.OrderBy(w => w.ProfitCentre),
                "description"     => descending ? query.OrderByDescending(w => w.Description)     : query.OrderBy(w => w.Description),
                "owner"           => descending ? query.OrderByDescending(w => w.Owner)           : query.OrderBy(w => w.Owner),
                "centraloverhead" => descending ? query.OrderByDescending(w => w.CentralOverhead) : query.OrderBy(w => w.CentralOverhead),
                _                 => query.OrderBy(w => w.WorkGroupName)
            };
        }
    }
}
