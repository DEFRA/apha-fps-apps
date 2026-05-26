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

        public async Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<PactProfitCentreView?> GetProfitCentreAsync(string profitCentre)
        {
            return await _context.PactProfitCentreViews
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProfitCentre == profitCentre);
        }

        public async Task<IEnumerable<WorkGroup>> GetWorkGroupsForEmailAsync(string profitCentre)
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.SendEmail == 1)
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeSheetTemplateRow>> GetTimeSheetTemplateAsync(
            string workGroup, short month, short layout)
        {
            if (layout == 2)
            {
                var flat = await (
                    from t  in _context.TimeCodeValids
                    join wg in _context.PactWorkGroupGradeViews on t.WorkGroup equals wg.WorkGroup
                    join s  in _context.WorkGroupStaffViews     on wg.WgGrade  equals s.WorkGroupGrade
                    join jc in _context.JobCodes      on t.JobCode  equals jc.JobCodeId into jcGroup
                    from jc in jcGroup.DefaultIfEmpty()
                    join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode into tpGroup
                    from tp in tpGroup.DefaultIfEmpty()
                    where t.WorkGroup == workGroup
                          && t.Active
                          && s.PersonStatus != "I"
                    orderby t.TimeCode, t.ParentProject, s.Name
                    select new
                    {
                        t.TimeCode,
                        t.ParentProject,
                        StaffName   = s.Name,
                        Description = jc != null ? jc.JobCodeName : tp.ItemDescription
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var rows = flat
                    .GroupBy(x => new { x.TimeCode, x.ParentProject })
                    .OrderBy(g => g.Key.TimeCode).ThenBy(g => g.Key.ParentProject)
                    .Select(g => new TimeSheetTemplateRow
                    {
                        StaffName     = string.Join(", ", g.Select(x => x.StaffName).Distinct().OrderBy(n => n)),
                        TimeCode      = g.Key.TimeCode,
                        Description   = g.Select(x => x.Description).FirstOrDefault(d => d != null),
                        ParentProject = g.Key.ParentProject,
                        Month         = month,
                        Hours         = null
                    })
                    .ToList();

                return rows;
            }
            else
            {
                var rows = await (
                    from t  in _context.TimeCodeValids
                    join wg in _context.PactWorkGroupGradeViews on t.WorkGroup equals wg.WorkGroup
                    join s  in _context.WorkGroupStaffViews     on wg.WgGrade   equals s.WorkGroupGrade
                    where t.WorkGroup == workGroup && t.Active
                    orderby t.WorkGroup, s.Name, t.TimeCode, t.ParentProject
                    select new TimeSheetTemplateRow
                    {
                        StaffName     = s.Name ?? string.Empty,
                        TimeCode      = t.TimeCode,
                        Description   = null,
                        ParentProject = t.ParentProject,
                        Month         = month,
                        Hours         = null
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return rows;
            }
        }

        public async Task<IEnumerable<OutputSheetTemplateRow>> GetOutputSheetTemplateAsync(
            string workGroup, short month)
        {
            var rows = await (
                from tc in _context.TestCapabilities
                join tr in _context.TestRequirements on tc.TestCode equals tr.TestCode
                join tp in _context.TestorProducts   on tc.TestCode equals tp.ItemCode
                where tc.WorkGroup == workGroup && tr.Active != 0
                orderby tc.TestCode, tr.Buyer
                select new OutputSheetTemplateRow
                {
                    TestCode        = tc.TestCode,
                    ItemDescription = tp.ItemDescription,
                    Buyer           = tr.Buyer,
                    Month           = month,
                    Volume          = null
                })
                .AsNoTracking()
                .ToListAsync();

            return rows;
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
                    ("WorkGroup",     true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    ("WorkGroup",     false) => baseQuery.OrderBy(e => e.WorkGroup),
                    ("ParentProject", true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    ("ParentProject", false) => baseQuery.OrderBy(e => e.ParentProject),
                    ("TimeCode",      true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    ("TimeCode",      false) => baseQuery.OrderBy(e => e.TimeCode),
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
                    ("WorkGroup",     true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    ("WorkGroup",     false) => baseQuery.OrderBy(e => e.WorkGroup),
                    ("TimeCode",      true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    ("TimeCode",      false) => baseQuery.OrderBy(e => e.TimeCode),
                    ("ParentProject", true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    ("ParentProject", false) => baseQuery.OrderBy(e => e.ParentProject),
                    ("Manager",       true)  => baseQuery.OrderByDescending(e => e.Manager),
                    ("Manager",       false) => baseQuery.OrderBy(e => e.Manager),
                    (_,               true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    _                        => baseQuery.OrderBy(e => e.ParentProject),
                };
            else
                baseQuery = baseQuery.OrderBy(e => e.ParentProject);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<WorkGroup>> GetWorkGroupsByProfitCentreAsync(
            PaginationParameters<string> query, string profitCentre)
        {
            var baseQuery = _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.FpsYear == _context.FilterFpsYear);

            baseQuery = ApplyWorkGroupFilter(baseQuery, query.Filter);

            // SendEmailYes / SendEmailNo are view-model-only computed properties that have no
            // corresponding column on the WorkGroup entity; fall back to WorkGroupName for those.
            var sortBy = query.SortBy is nameof(WorkGroup.WorkGroupName) or nameof(WorkGroup.EmailRecipient)
                ? query.SortBy
                : nameof(WorkGroup.WorkGroupName);

            baseQuery = query.Descending
                ? baseQuery.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : baseQuery.OrderBy(e => EF.Property<object>(e, sortBy));

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(wg => wg.FpsYear == fpsYear
                          && _context.ProfitCentres
                                .Any(pc => pc.ProfitCentreId == profitCentre
                                        && pc.ProfitCentreId == wg.ProfitCentre))
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return true;
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(w => w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return true;
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(w => w.WorkGroupName == workGroupName && w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(w => w.SendEmail, sendEmail)
                    .SetProperty(w => w.EmailRecipient, emailRecipient));
            return true;
        }

        private static IQueryable<WorkGroup> ApplyWorkGroupFilter(
            IQueryable<WorkGroup> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("WorkGroupName", out var workGroupName) && !string.IsNullOrWhiteSpace(workGroupName))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{workGroupName}%"));

            if (filters.TryGetValue("EmailRecipient", out var emailRecipient) && !string.IsNullOrWhiteSpace(emailRecipient))
                query = query.Where(w => w.EmailRecipient != null &&
                                         EF.Functions.ILike(w.EmailRecipient, $"%{emailRecipient}%"));

            return query;
        }

        private static IQueryable<WorkGroupValidTimeCode> ApplyWorkGroupValidTimeCodeFilter(
            IQueryable<WorkGroupValidTimeCode> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("WorkGroup", out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup, $"%{workGroup}%"));

            if (filters.TryGetValue("TimeCode", out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            if (filters.TryGetValue("ParentProject", out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
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

            if (filters.TryGetValue("WorkGroup", out var workGroupFilter) && !string.IsNullOrWhiteSpace(workGroupFilter))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup!, $"%{workGroupFilter}%"));

            if (filters.TryGetValue("ParentProject", out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(e => EF.Functions.ILike(e.ParentProject, $"%{parentProject}%"));

            if (filters.TryGetValue("TimeCode", out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            return query;
        }
    }
}
