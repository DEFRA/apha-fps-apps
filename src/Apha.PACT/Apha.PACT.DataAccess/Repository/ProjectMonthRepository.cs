using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProjectMonthRepository : BaseRepository, IProjectMonthRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public ProjectMonthRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<IList<Month>> GetMonthsAsync()
        {
            return await _context.Months
                .AsNoTracking()
                 .Select(m => new Month
                 {
                     AccntsPeriod = m.AccntsPeriod,
                     MonthName = m.MonthName
                 })
                .ToListAsync();
        }

        public async Task<IList<ProjectMonth>> GetProjectMonthByProjectAsync(string project)
        {
            return await _context.ProjectMonths
                .AsNoTracking()
                .Where(e => e.Project == project)
                .OrderBy(e => e.MonthNo)
                .ToListAsync();
        }

        public async Task<ProjectMonth?> GetProjectMonthAsync(string project, int monthNo)
        {
            return await _context.ProjectMonths
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Project == project && e.MonthNo == monthNo);
        }

        public async Task<ProjectMonth> CreateProjectMonthAsync(ProjectMonth entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.ProjectMonths.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProjectMonth> UpdateProjectMonthAsync(ProjectMonth entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteProjectMonthAsync(string project, int monthNo)
        {
            ProjectMonth? entity = await _context.ProjectMonths
                .FirstOrDefaultAsync(
                e => e.Project == project
                && e.MonthNo == monthNo
                && e.FpsYear == _fpsRequestContext.FpsYear);

            if (entity == null) return false;
            _context.ProjectMonths.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}