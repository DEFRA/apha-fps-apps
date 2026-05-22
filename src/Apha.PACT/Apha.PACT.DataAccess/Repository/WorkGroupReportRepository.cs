using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class WorkGroupReportRepository : BaseRepository, IWorkGroupReportRepository
    {
        public WorkGroupReportRepository(FpsDbContext context) : base(context) { }

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
                        // IIf(IsNull(JobCode), ItemDescription, JobCodeName)
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
                    Volume          = null          // blank template
                })
                .AsNoTracking()
                .ToListAsync();

            return rows;
        }
    }
}
