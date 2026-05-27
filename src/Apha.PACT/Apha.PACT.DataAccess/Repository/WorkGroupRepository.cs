using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        public WorkGroupRepository(FpsDbContext context) : base(context)
        {
        }

        private const string WorkGroupColumn     = "WorkGroup";
        private const string ParentProjectColumn = "ParentProject";
        private const string TimeCodeColumn      = "TimeCode";

        public async Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<PagedData<WorkGroupTimeCode>> GetWorkGroupTimeCodeAsync(
            PaginationParameters<string> query, string? workGroup, int? monthNumber)
        {
            var baseQuery = _context.PactWorkGroupGradeViews
                .Join(_context.WorkGroupStaffViews,
                    gradeView => gradeView.WgGrade,
                    staff => staff.WorkGroupGrade,
                    (gradeView, staff) => new { gradeView, staff })
                .Join(_context.MonthlyTimes,
                    gradeStaff => gradeStaff.staff.PactId,
                    timeRecord => timeRecord.PactStaffId,
                    (gradeStaff, timeRecord) => new WorkGroupTimeCode
                    {
                        PACTStaffID   = timeRecord.PactStaffId,
                        ParentProject = timeRecord.ParentProject,
                        WorkGroup     = gradeStaff.gradeView.WorkGroup,
                        Name          = gradeStaff.staff.Name,
                        TimeCode      = timeRecord.TimeCode,
                        Month         = timeRecord.Month,
                        Hours         = timeRecord.Hours ?? 0
                    })
                .Distinct()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(e => e.WorkGroup == workGroup);

            if (monthNumber.HasValue)
                baseQuery = baseQuery.Where(e => e.Month == monthNumber.Value);

            baseQuery = ApplyWorkGroupTimeCodeFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = (query.SortBy, query.Descending) switch
                {
                    ("PACTStaffID",   true)  => baseQuery.OrderByDescending(e => e.PACTStaffID),
                    ("PACTStaffID",   false) => baseQuery.OrderBy(e => e.PACTStaffID),
                    (WorkGroupColumn,      true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    (WorkGroupColumn,      false) => baseQuery.OrderBy(e => e.WorkGroup),
                    (ParentProjectColumn,  true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    (ParentProjectColumn,  false) => baseQuery.OrderBy(e => e.ParentProject),
                    (TimeCodeColumn,       true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    (TimeCodeColumn,       false) => baseQuery.OrderBy(e => e.TimeCode),
                    ("Month",         true)  => baseQuery.OrderByDescending(e => e.Month),
                    ("Month",         false) => baseQuery.OrderBy(e => e.Month),
                    ("Hours",         true)  => baseQuery.OrderByDescending(e => e.Hours),
                    ("Hours",         false) => baseQuery.OrderBy(e => e.Hours),
                    (_,               true)  => baseQuery.OrderByDescending(e => e.Name),
                    _                        => baseQuery.OrderBy(e => e.Name),
                };
            else
                baseQuery = baseQuery.OrderBy(e => e.Name);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<WorkGroupValidTimeCode>> GetWorkGroupValidTimeCodeAsync(
            PaginationParameters<string> query, string workGroup)
        {
            var baseQuery = _context.TimeCodeValids
                .AsNoTracking()
                .Join(_context.Projects.AsNoTracking(),
                    timeCodeValid => timeCodeValid.ParentProject,
                    project       => project.ParentProject,
                    (timeCodeValid, project) => new WorkGroupValidTimeCode
                    {
                        WorkGroup     = timeCodeValid.WorkGroup,
                        TimeCode      = timeCodeValid.TimeCode,
                        ParentProject = timeCodeValid.ParentProject,
                        Manager       = project.Manager,
                        Active        = timeCodeValid.Active
                    });

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(e => e.WorkGroup == workGroup);

            baseQuery = ApplyWorkGroupValidTimeCodeFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = (query.SortBy, query.Descending) switch
                {
                    (WorkGroupColumn,      true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    (WorkGroupColumn,      false) => baseQuery.OrderBy(e => e.WorkGroup),
                    (TimeCodeColumn,       true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    (TimeCodeColumn,       false) => baseQuery.OrderBy(e => e.TimeCode),
                    (ParentProjectColumn,  true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    (ParentProjectColumn,  false) => baseQuery.OrderBy(e => e.ParentProject),
                    ("Manager",            true)  => baseQuery.OrderByDescending(e => e.Manager),
                    ("Manager",            false) => baseQuery.OrderBy(e => e.Manager),
                    (_,                    true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    _                             => baseQuery.OrderBy(e => e.ParentProject),
                };
            else
                baseQuery = baseQuery.OrderBy(e => e.ParentProject);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<WgSummarisedStaffTimeUsageView>> GetWgSummarisedStaffTimeUsageAsync(
            string workGroup)
        {
            return await _context.WgSummarisedStaffTimeUsageViews
                .AsNoTracking()
                .Where(e => e.WorkGroup == workGroup)
                .ToListAsync();
        }

        private static IQueryable<WorkGroupValidTimeCode> ApplyWorkGroupValidTimeCodeFilter(
            IQueryable<WorkGroupValidTimeCode> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(WorkGroupColumn, out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup, $"%{workGroup}%"));

            if (filters.TryGetValue(TimeCodeColumn, out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            if (filters.TryGetValue(ParentProjectColumn, out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(e => EF.Functions.ILike(e.ParentProject, $"%{parentProject}%"));

            if (filters.TryGetValue("Manager", out var manager) && !string.IsNullOrWhiteSpace(manager))
                query = query.Where(e => EF.Functions.ILike(e.Manager!, $"%{manager}%"));

            return query;
        }

        private static IQueryable<WorkGroupTimeCode> ApplyWorkGroupTimeCodeFilter(
            IQueryable<WorkGroupTimeCode> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("PACTStaffID", out var pactStaffId) && !string.IsNullOrWhiteSpace(pactStaffId))
                query = query.Where(e => EF.Functions.ILike(e.PACTStaffID, $"%{pactStaffId}%"));

            if (filters.TryGetValue("Name", out var name) && !string.IsNullOrWhiteSpace(name))
                query = query.Where(e => EF.Functions.ILike(e.Name!, $"%{name}%"));

            if (filters.TryGetValue(WorkGroupColumn, out var workGroupFilter) && !string.IsNullOrWhiteSpace(workGroupFilter))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup!, $"%{workGroupFilter}%"));

            if (filters.TryGetValue(ParentProjectColumn, out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(e => EF.Functions.ILike(e.ParentProject, $"%{parentProject}%"));

            if (filters.TryGetValue(TimeCodeColumn, out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            return query;
        }
    }
}
