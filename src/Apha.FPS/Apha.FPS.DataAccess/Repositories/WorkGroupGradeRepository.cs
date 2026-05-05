using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WorkGroupGradeRepository : BaseRepository, IWorkGroupGradeRepository
    {
        private readonly FpsDbContext _dbContext;

        public WorkGroupGradeRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a paginated list of WG grades for the given PC grade.
        /// </summary>
        public async Task<PagedData<WorkgroupGrade>> GetWorkGroupGradeAsync(PaginationParameters<string> query, string pcGrade)
        {
            var all = await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .Where(x => x.ProfitCentreGrade == pcGrade)
                .OrderBy(x => x.WgGrade)
                .ToListAsync(default);

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        /// <summary>
        /// Deletes a WG grade by its grade code.
        /// </summary>
        public async Task DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var entity = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(x => x.WgGrade == wgGrade);
            if (entity == null)
                throw new KeyNotFoundException($"WG grade '{wgGrade}' was not found.");

            _dbContext.WorkgroupGrades.Remove(entity);
            await _dbContext.SaveChangesAsync(default);
        }
    }
}
