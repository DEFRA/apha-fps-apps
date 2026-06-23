/*
 * TRANSFORMENGINE MIGRATION — WorkgroupRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New repository created; no prior WorkgroupRepository existed in this codebase
 *   - Implements IWorkgroupRepository (9 async methods) backed by FpsDbContext
 *   - GetPagedAsync: LINQ filter/sort over fps.workgroup (DbContext HasQueryFilter scopes
 *     results to the active FPS year automatically)
 *   - GetByKeyAsync: AsNoTracking FirstOrDefaultAsync on WorkGroupName
 *   - CreateAsync: FpsYear stamped from IFpsRequestContext before Add + SaveChangesAsync
 *   - UpdateAsync: supports PK rename (originalWorkGroupName param); applies field-by-field
 *     update including WorkGroupName rename; preserves FpsYear via DbContext filter
 *   - DeleteAsync: ExecuteDeleteAsync for set-based remove (no entity load required)
 *   - ExistsAsync: AnyAsync guard for duplicate-name validation before Create
 *   - GetAllProfitCentresAsync: distinct ProfitCentreId values from fps.tblkpprofitcentre
 *     (ProfitCentres DbSet) ordered alphabetically
 *   - GetOwnersAsync: LINQ translation of fps/qryManager MS Access named query —
 *     joins vtblstaffactive (StaffActiveView) with vworkgroupgrade_general
 *     (WorkgroupGradeGeneralViews) on WorkgroupGrade == WgGrade; filters out
 *     name like "general"/"vacancy" and GradeCode starting with "G";
 *     projects to Manager { Name, WorkGroup, GradeCode, Expr1 = GradeCode[0] }
 *   - GetCostCentresByProfitCentreAsync: distinct CostCentre values from fps.workgroup
 *     for a given ProfitCentre (FpsYear-filtered by DbContext)
 *
 * PRESERVED:
 *   - FpsYear scoping via DbContext HasQueryFilter (no manual year filter in repository)
 *   - All conditional null guards and ArgumentNullException patterns consistent with
 *     other FPS repositories in this project
 *   - AsNoTracking for all read-only queries
 *   - AnyAsync for existence checks
 *   - ApplyPaging helper inherited from BaseRepository
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetOwnersAsync — qryManager uses DISTINCTROW (Access dialect);
 *     Distinct() in LINQ should be equivalent but verify result set matches expected
 *     Manager dropdown in the UI
 *   - TRANSFORMENGINE TODO: UpdateAsync supports WorkGroupName rename; verify whether the
 *     legacy frmMaintWorkGroup2 form actually allowed renaming the primary key — if not,
 *     the originalWorkGroupName parameter can be collapsed to a single entity param
 *   - TRANSFORMENGINE TODO: GetCostCentresByProfitCentreAsync — CostCentre is double? in
 *     the DDL; confirm that returning IEnumerable<double?> is sufficient for the UI dropdown
 *     or whether a labelled projection (value + display text) is needed
 */
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

        // TRANSFORMENGINE: GetPagedAsync — paged list scoped to active FPS year via DbContext HasQueryFilter;
        // filter covers WorkGroupName, ProfitCentre, and Description (matches planned route surface)
        /// <inheritdoc/>
        public async Task<PagedData<Workgroup>> GetPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = _dbContext.Workgroups
                .AsNoTracking()
                .AsQueryable();

            // TRANSFORMENGINE: Apply JSON filter model (WorkGroupName, ProfitCentre, Description)
            baseQuery = ApplyWorkgroupFilter(baseQuery, query.Filter);

            // TRANSFORMENGINE: Apply sort; default to WorkGroupName ascending
            baseQuery = ApplyWorkgroupSorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetByKeyAsync — look up single workgroup by WorkGroupName;
        // FpsYear resolved automatically via DbContext HasQueryFilter
        /// <inheritdoc/>
        public async Task<Workgroup?> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return null;

            return await _dbContext.Workgroups
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkGroupName == workGroupName);
        }

        // TRANSFORMENGINE: CreateAsync — stamps FpsYear from IFpsRequestContext before insert
        /// <inheritdoc/>
        public async Task<Workgroup> CreateAsync(Workgroup workgroup)
        {
            ArgumentNullException.ThrowIfNull(workgroup);

            // TRANSFORMENGINE: FpsYear must match the active planning year for the HasQueryFilter to include this row
            workgroup.FpsYear = _requestContext.FpsYear;

            _dbContext.Workgroups.Add(workgroup);
            await _dbContext.SaveChangesAsync();
            return workgroup;
        }

        // TRANSFORMENGINE: UpdateAsync — supports PK rename (originalWorkGroupName → workgroup.WorkGroupName);
        // loads tracked entity, updates all mutable fields, saves
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

            // TRANSFORMENGINE: Apply all mutable field updates; WorkGroupName rename supported (PK value change)
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
            // TRANSFORMENGINE: FpsYear is the partition key — do not overwrite from caller; keep active year
            existing.FpsYear          = _requestContext.FpsYear;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // TRANSFORMENGINE: DeleteAsync — ExecuteDeleteAsync for efficient set-based delete without entity load
        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            // TRANSFORMENGINE: HasQueryFilter on DbContext ensures only the active-year row is deleted
            var deleted = await _dbContext.Workgroups
                .Where(w => w.WorkGroupName == workGroupName)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync guard used before CreateAsync to prevent duplicates
        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            return await _dbContext.Workgroups
                .AsNoTracking()
                .AnyAsync(w => w.WorkGroupName == workGroupName);
        }

        // TRANSFORMENGINE: GetAllProfitCentresAsync — returns distinct ProfitCentreId values from
        // fps.tblkpprofitcentre (ProfitCentres DbSet); ProfitCentre entity is not year-filtered so
        // all active profit centres are available regardless of FpsYear
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

        // TRANSFORMENGINE: GetOwnersAsync — LINQ translation of fps/qryManager named query:
        //   SELECT DISTINCTROW tblStaffActive.Name, WorkGroupGrade_General.WorkGroup,
        //          WorkGroupGrade_General.GradeCode, Left([gradecode],1) AS Expr1
        //   FROM tblStaffActive
        //   INNER JOIN WorkGroupGrade_General ON tblStaffActive.WorkGroupGrade = WorkGroupGrade_General.WGGrade
        //   WHERE Name Not Like "*general*" And Name Not Like "*vacancy*"
        //     AND Left([gradecode],1) <> "G"
        //   ORDER BY tblStaffActive.Name
        /// <inheritdoc/>
        public async Task<IEnumerable<Manager>> GetOwnersAsync()
        {
            // TRANSFORMENGINE: Join vtblstaffactive (StaffActiveView) with vworkgroupgrade_general
            // (WorkgroupGradeGeneralViews) on WorkgroupGrade = WgGrade; FpsYear filter applied by
            // DbContext HasQueryFilter on WorkgroupGradeGeneralViews; StaffActiveView is also year-filtered
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
                // TRANSFORMENGINE: WHERE Name Not Like "*general*" And Name Not Like "*vacancy*"
                .Where(x => x.Name != null
                         && !x.Name.ToLower().Contains("general")
                         && !x.Name.ToLower().Contains("vacancy"))
                // TRANSFORMENGINE: AND Left([gradecode],1) <> "G"
                .Where(x => x.GradeCode != null && !x.GradeCode.StartsWith("G"))
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => new Manager
                {
                    Name      = x.Name,
                    WorkGroup = x.WorkGroup,
                    GradeCode = x.GradeCode,
                    // TRANSFORMENGINE: Left([gradecode],1) AS Expr1 — first character of GradeCode
                    Expr1     = x.GradeCode != null && x.GradeCode.Length > 0
                                    ? x.GradeCode.Substring(0, 1)
                                    : null
                })
                .ToListAsync();

            return result;
        }

        // TRANSFORMENGINE: GetCostCentresByProfitCentreAsync — distinct CostCentre (double?) values
        // from fps.workgroup for a given ProfitCentre; FpsYear resolved via DbContext HasQueryFilter
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

        // TRANSFORMENGINE: ApplyWorkgroupFilter — JSON-deserialized filter supports WorkGroupName,
        // ProfitCentre, and Description field-level contains searches
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

        // TRANSFORMENGINE: ApplyWorkgroupSorting — sort by WorkGroupName, ProfitCentre, Description,
        // Owner, or CentralOverhead; default is WorkGroupName ascending
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
