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
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupGradeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }
       
        public async Task<PagedData<WorkGroupGradeView>> GetWorkGroupGradesAsync(
            PaginationParameters<string> query,
            string profitCentreGrade)
        {
            var all = await _dbContext.WorkGroupGradeViews
                .AsNoTracking()
                .Where(x => x.ProfitCentreGrade == profitCentreGrade
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .OrderBy(x => x.WgGrade)
                .ToListAsync();

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        public async Task<bool> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var entity = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(x => x.WgGrade == wgGrade);
            if (entity == null)
                return false;

            _dbContext.WorkgroupGrades.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
