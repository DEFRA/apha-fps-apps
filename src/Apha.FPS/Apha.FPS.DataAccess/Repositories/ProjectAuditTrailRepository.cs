using System.Linq.Expressions;
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

        public ProjectAuditTrailRepository(FpsDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // fps.project_log has a direct parentproject column; no join required
        public async Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = _dbContext.ProjectLogs
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(p => p.ParentProject == parentProject);

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

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

        // join to fps.tlkpjob (JobCodes) on jobcode to resolve the parentproject association
        public async Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.StaffJobLogs.AsNoTracking().IgnoreQueryFilters()
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

        // jobcode column in testreq_log is derived from projectbuyercode; join to JobCodes on jobcode
        public async Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.TestRequirementLogs.AsNoTracking().IgnoreQueryFilters()
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

        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.AnimalRequestLogs.AsNoTracking().IgnoreQueryFilters()
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

        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.AdditionalCostLogs.AsNoTracking().IgnoreQueryFilters()
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

        private static IQueryable<TestRequirementLog> ApplyTestRequirementLogSorting(
            IQueryable<TestRequirementLog> q, string? sortBy, bool descending)
        {
            Expression<Func<TestRequirementLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "testcode"         => e => e.TestCode,
                "buyer"            => e => e.Buyer,
                "unitprice"        => e => e.UnitPrice,
                "norequired"       => e => e.NoRequired,
                "projectbuyercode" => e => e.ProjectBuyerCode,
                "testbuyercode"    => e => e.TestBuyerCode,
                "active"           => e => e.Active,
                "date_time"        => e => e.DateTime,
                "insert_delete"    => e => e.InsertDelete,
                "user_id"          => e => e.UserId,
                _                  => e => e.DateTime,
            };
            return descending ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);
        }

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

        private static IQueryable<AdditionalCostLog> ApplyAdditionalCostLogSorting(
            IQueryable<AdditionalCostLog> q, string? sortBy, bool descending)
        {
            Expression<Func<AdditionalCostLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "jobcode"       => e => e.JobCode,
                "account"       => e => e.Account,
                "description"   => e => e.Description,
                "itemcost"      => e => e.ItemCost,
                "freq"          => e => e.Freq,
                "supplier"      => e => e.Supplier,
                "date_time"     => e => e.DateTime,
                "insert_delete" => e => e.InsertDelete,
                "user_id"       => e => e.UserId,
                _               => e => e.DateTime,
            };
            return descending ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);
        }
    }
}
