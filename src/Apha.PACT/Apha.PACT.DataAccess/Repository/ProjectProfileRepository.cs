using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProjectProfileRepository(FpsDbContext context) :
        BaseRepository(context),
        IProjectProfileRepository
    {
        public async Task<IList<ProjectProfile>> GetProfileDataAsync(string project)
        {
            return await _context.ProjectMonthFinals
                .AsNoTracking()
                .Where(pmf => pmf.Project == project)
                .Join(
                    _context.ProjectMonths.AsNoTracking(),
                    pmf => new { pmf.Project, MonthNo = (double?)pmf.MonthNo, FpsYear = (int?)pmf.FpsYear },
                    pm => new { pm.Project, MonthNo = (double?)pm.MonthNo, FpsYear = (int?)pm.FpsYear },
                    (pmf, pm) => new ProjectProfile
                    {
                        MonthNo = pmf.MonthNo,
                        Profile = pm.CostProfile,
                        Cost = pmf.TotalCost
                    })
                .OrderBy(p => p.MonthNo)
                .ToListAsync();
        }

        public async Task<IList<ProjectProfile>> GetCumulativeDataAsync(string project)
        {
            return await _context.ProjectMonthFinals
                .AsNoTracking()
                .Where(pmf => pmf.Project == project)
                .Join(
                    _context.ProjectMonths.AsNoTracking().Where(pm => pm.Project == project),
                    pmf => new { pmf.Project, FpsYear = (int?)pmf.FpsYear },
                    pm => new { pm.Project, FpsYear = (int?)pm.FpsYear },
                    (pmf, pm) => new { pmf, pm })
                .Join(
                    _context.PeriodMonths.AsNoTracking(),
                    j => new { MonthNo = (double?)j.pm.MonthNo, EndMonth = (double?)j.pmf.MonthNo, FpsYear = j.pmf.FpsYear },
                    p => new { p.MonthNo, p.EndMonth, p.FpsYear },
                    (j, p) => new { j.pmf, j.pm })
                .GroupBy(j => new { j.pmf.MonthNo, j.pmf.CumCost })
                .Select(g => new ProjectProfile
                {
                    MonthNo = g.Key.MonthNo,
                    Profile = g.Sum(j => j.pm.CostProfile),
                    Cost = g.Key.CumCost
                })
                .OrderBy(p => p.MonthNo)
                .ToListAsync();
        }
    }
}