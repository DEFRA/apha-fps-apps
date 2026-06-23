/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — no legacy equivalent (MS Access queried log tables directly via DAO/ADODB RecordSets)
 *   - Created LINQ-first repository implementing IProjectAuditTrailRepository
 *   - 5 async query methods: GetProjectLogsAsync, GetStaffJobLogsAsync, GetTestRequirementLogsAsync,
 *     GetAnimalRequestLogsAsync, GetAdditionalCostLogsAsync
 *   - All methods use AsNoTracking() for read-only queries
 *   - FpsYear filtering applied via DbContext HasQueryFilter (FpsDbContext.FilterFpsYear)
 *   - StaffJobLog/AnimalRequestLog/AdditionalCostLog/TestRequirementLog do not have a direct ParentProject
 *     column — filtered via join to JobCodes table (JobCode.ParentProject == parentProject,
 *     JobCode.JobCodeId == log.JobCode)
 *   - Optional date range applied as nullable DateTime guards on DateTime column
 *   - Sorting applied via private static helper using switch expression on SortBy field name
 *   - Pagination delegated to BaseRepository.ApplyPaging()
 *
 * PRESERVED:
 *   - IProjectAuditTrailRepository interface signatures exactly (5 methods, same param types)
 *   - Async-only pattern consistent with all other FPS repositories
 *   - IFpsRequestContext injection (provides FpsYear for context — used indirectly via HasQueryFilter)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: TestRequirementLog.JobCode is derived from ProjectBuyerCode in DDL comment;
 *     the join uses JobCode.JobCodeId == log.JobCode — verify this is the correct FK path for testreq_log
 *   - TRANSFORMENGINE TODO: StaffJobLog has no index on ParentProject — the join to JobCodes may be slow
 *     for large datasets; consider adding a covering index on fps.staffjob_log(jobcode, fpsyear)
 *
 * Phase 14 Security Review — PASS: Pure LINQ throughout all 5 query methods (no raw SQL); all
 *   user-supplied values (parentProject, fromDate, toDate, query.Search) are passed through EF Core
 *   parameterization — never string-concatenated into queries; SortBy is channeled through a closed
 *   switch expression with a safe default (no sort-field injection vector); AsNoTracking() applied on all
 *   read paths; FpsYear scoping via HasQueryFilter cannot be bypassed; no hardcoded secrets or
 *   credentials; no security defects found — no code changes required.
 */
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectAuditTrailRepository : BaseRepository, IProjectAuditTrailRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProjectAuditTrailRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        // TRANSFORMENGINE: GetProjectLogsAsync — queries fps.project_log filtered by ParentProject + optional date range
        // fps.project_log has a direct parentproject column; no join required
        public async Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = _dbContext.ProjectLogs
                .AsNoTracking()
                .Where(p => p.ParentProject == parentProject);

            // TRANSFORMENGINE: optional date range applied on date_time column
            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            // TRANSFORMENGINE: search across key text columns when Search is provided
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    (p.InsertDelete != null && p.InsertDelete.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyProjectLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetStaffJobLogsAsync — fps.staffjob_log has no parentproject column;
        // join to fps.tlkpjob (JobCodes) on jobcode to resolve the parentproject association
        public async Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // TRANSFORMENGINE: join strategy — StaffJobLog.JobCode → JobCode.JobCodeId → JobCode.ParentProject
            var q = from log in _dbContext.StaffJobLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyStaffJobLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetTestRequirementLogsAsync — fps.testreq_log has no parentproject column;
        // jobcode column in testreq_log is derived from projectbuyercode; join to JobCodes on jobcode
        public async Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // TRANSFORMENGINE: join strategy — TestRequirementLog.JobCode → JobCode.JobCodeId → JobCode.ParentProject
            var q = from log in _dbContext.TestRequirementLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    (p.TestCode != null && p.TestCode.ToLower().Contains(search)) ||
                    (p.Buyer != null && p.Buyer.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyTestRequirementLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetAnimalRequestLogsAsync — fps.animalreq_log has no parentproject column;
        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // TRANSFORMENGINE: join strategy — AnimalRequestLog.JobCode → JobCode.JobCodeId → JobCode.ParentProject
            var q = from log in _dbContext.AnimalRequestLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.AnimalType.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyAnimalRequestLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetAdditionalCostLogsAsync — fps.additionalcosts_log has no parentproject column;
        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // TRANSFORMENGINE: join strategy — AdditionalCostLog.JobCode → JobCode.JobCodeId → JobCode.ParentProject
            var q = from log in _dbContext.AdditionalCostLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.Account.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyAdditionalCostLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // ── Private sorting helpers ──────────────────────────────────────────────────────

        // TRANSFORMENGINE: sorting helper for ProjectLog — switch on SortBy field name
        private static IQueryable<ProjectLog> ApplyProjectLogSorting(
            IQueryable<ProjectLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "parentproject" => descending ? q.OrderByDescending(e => e.ParentProject) : q.OrderBy(e => e.ParentProject),
                "projecttitle"  => descending ? q.OrderByDescending(e => e.ProjectTitle)  : q.OrderBy(e => e.ProjectTitle),
                "program"       => descending ? q.OrderByDescending(e => e.Program)        : q.OrderBy(e => e.Program),
                "jobcode"       => descending ? q.OrderByDescending(e => e.JobCode)        : q.OrderBy(e => e.JobCode),
                "date_time"     => descending ? q.OrderByDescending(e => e.DateTime)       : q.OrderBy(e => e.DateTime),
                "insert_delete" => descending ? q.OrderByDescending(e => e.InsertDelete)   : q.OrderBy(e => e.InsertDelete),
                "user_id"       => descending ? q.OrderByDescending(e => e.UserId)         : q.OrderBy(e => e.UserId),
                _               => q.OrderByDescending(e => e.DateTime),
            };
        }

        // TRANSFORMENGINE: sorting helper for StaffJobLog
        private static IQueryable<StaffJobLog> ApplyStaffJobLogSorting(
            IQueryable<StaffJobLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "staffid"       => descending ? q.OrderByDescending(e => e.StaffId)      : q.OrderBy(e => e.StaffId),
                "jobcode"       => descending ? q.OrderByDescending(e => e.JobCode)      : q.OrderBy(e => e.JobCode),
                "plannedhours"  => descending ? q.OrderByDescending(e => e.PlannedHours) : q.OrderBy(e => e.PlannedHours),
                "date_time"     => descending ? q.OrderByDescending(e => e.DateTime)     : q.OrderBy(e => e.DateTime),
                "insert_delete" => descending ? q.OrderByDescending(e => e.InsertDelete) : q.OrderBy(e => e.InsertDelete),
                "user_id"       => descending ? q.OrderByDescending(e => e.UserId)       : q.OrderBy(e => e.UserId),
                _               => q.OrderByDescending(e => e.DateTime),
            };
        }

        // TRANSFORMENGINE: sorting helper for TestRequirementLog
        private static IQueryable<TestRequirementLog> ApplyTestRequirementLogSorting(
            IQueryable<TestRequirementLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "testcode"         => descending ? q.OrderByDescending(e => e.TestCode)         : q.OrderBy(e => e.TestCode),
                "buyer"            => descending ? q.OrderByDescending(e => e.Buyer)            : q.OrderBy(e => e.Buyer),
                "unitprice"        => descending ? q.OrderByDescending(e => e.UnitPrice)        : q.OrderBy(e => e.UnitPrice),
                "norequired"       => descending ? q.OrderByDescending(e => e.NoRequired)       : q.OrderBy(e => e.NoRequired),
                "projectbuyercode" => descending ? q.OrderByDescending(e => e.ProjectBuyerCode) : q.OrderBy(e => e.ProjectBuyerCode),
                "testbuyercode"    => descending ? q.OrderByDescending(e => e.TestBuyerCode)    : q.OrderBy(e => e.TestBuyerCode),
                "active"           => descending ? q.OrderByDescending(e => e.Active)           : q.OrderBy(e => e.Active),
                "date_time"        => descending ? q.OrderByDescending(e => e.DateTime)         : q.OrderBy(e => e.DateTime),
                "insert_delete"    => descending ? q.OrderByDescending(e => e.InsertDelete)     : q.OrderBy(e => e.InsertDelete),
                "user_id"          => descending ? q.OrderByDescending(e => e.UserId)           : q.OrderBy(e => e.UserId),
                _                  => q.OrderByDescending(e => e.DateTime),
            };
        }

        // TRANSFORMENGINE: sorting helper for AnimalRequestLog
        private static IQueryable<AnimalRequestLog> ApplyAnimalRequestLogSorting(
            IQueryable<AnimalRequestLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "jobcode"         => descending ? q.OrderByDescending(e => e.JobCode)         : q.OrderBy(e => e.JobCode),
                "animaltype"      => descending ? q.OrderByDescending(e => e.AnimalType)      : q.OrderBy(e => e.AnimalType),
                "numberofdays"    => descending ? q.OrderByDescending(e => e.NumberOfDays)    : q.OrderBy(e => e.NumberOfDays),
                "numberofanimals" => descending ? q.OrderByDescending(e => e.NumberOfAnimals) : q.OrderBy(e => e.NumberOfAnimals),
                "date_time"       => descending ? q.OrderByDescending(e => e.DateTime)        : q.OrderBy(e => e.DateTime),
                "insert_delete"   => descending ? q.OrderByDescending(e => e.InsertDelete)    : q.OrderBy(e => e.InsertDelete),
                "user_id"         => descending ? q.OrderByDescending(e => e.UserId)          : q.OrderBy(e => e.UserId),
                _                 => q.OrderByDescending(e => e.DateTime),
            };
        }

        // TRANSFORMENGINE: sorting helper for AdditionalCostLog
        private static IQueryable<AdditionalCostLog> ApplyAdditionalCostLogSorting(
            IQueryable<AdditionalCostLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "jobcode"       => descending ? q.OrderByDescending(e => e.JobCode)     : q.OrderBy(e => e.JobCode),
                "account"       => descending ? q.OrderByDescending(e => e.Account)     : q.OrderBy(e => e.Account),
                "description"   => descending ? q.OrderByDescending(e => e.Description) : q.OrderBy(e => e.Description),
                "itemcost"      => descending ? q.OrderByDescending(e => e.ItemCost)    : q.OrderBy(e => e.ItemCost),
                "freq"          => descending ? q.OrderByDescending(e => e.Freq)        : q.OrderBy(e => e.Freq),
                "supplier"      => descending ? q.OrderByDescending(e => e.Supplier)    : q.OrderBy(e => e.Supplier),
                "date_time"     => descending ? q.OrderByDescending(e => e.DateTime)    : q.OrderBy(e => e.DateTime),
                "insert_delete" => descending ? q.OrderByDescending(e => e.InsertDelete): q.OrderBy(e => e.InsertDelete),
                "user_id"       => descending ? q.OrderByDescending(e => e.UserId)      : q.OrderBy(e => e.UserId),
                _               => q.OrderByDescending(e => e.DateTime),
            };
        }
    }
}
