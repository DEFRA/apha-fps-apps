using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WgGradeRepository : BaseRepository, IWgGradeRepository
    {
        private readonly FpsDbContext _dbContext;

        public WgGradeRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a paginated list of WG grades for the given PC grade.
        /// </summary>
        public async Task<PagedData<WorkgroupGrade>> GetWgGradesAsync(PaginationParameters<string> query, string pcGrade, CancellationToken cancellationToken = default)
        {
            var all = await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .Where(x => x.ProfitCentreGrade == pcGrade)
                .OrderBy(x => x.WgGrade)
                .ToListAsync(cancellationToken);

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        /// <summary>
        /// Deletes a WG grade by its grade code.
        /// </summary>
        public async Task DeleteWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(x => x.WgGrade == wgGrade, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"WG grade '{wgGrade}' was not found.");

            _dbContext.WorkgroupGrades.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
