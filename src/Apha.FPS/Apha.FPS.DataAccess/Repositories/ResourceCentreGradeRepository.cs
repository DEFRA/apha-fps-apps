using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ResourceCentreGradeRepository : BaseRepository, IResourceCentreGradeRepository
    {
        private readonly FpsDbContext _dbContext;

        public ResourceCentreGradeRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a paginated list of RC grades for the given profit centre.
        /// </summary>
        public async Task<PagedData<ProfitCentreGrade>> GetResourceCentreGradesAsync(PaginationParameters<string> query, string profitCentre)
        {
            var all = await _dbContext.ProfitcentreGrades
                .AsNoTracking()
                .Where(x => x.ProfitCentre == profitCentre)
                .OrderByDescending(x => x.ChargeRate)
                .ToListAsync(default);

            return ApplyPaging(all, query.Page, query.PageSize);
        }
    }
}
