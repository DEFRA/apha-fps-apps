using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public MonthlyTimeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<PagedData<MonthlyTime>> GetByProjectAsync(
            PaginationParameters<string> query, string parentProject)
        {
            var result = await _dbContext.MonthlyTimes
                .AsNoTracking()
                .Where(x => x.ParentProject == parentProject)
                .ToListAsync();

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<MonthlyTime?> GetByKeyAsync(
            string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _dbContext.MonthlyTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.PactStaffId    == pactStaffId &&
                    x.TimeCode       == timeCode    &&
                    x.Month          == month       &&
                    x.ParentProject  == parentProject);
        }

        public async Task<MonthlyTime> UpsertAsync(MonthlyTime entity)
        {
            var existing = await _dbContext.MonthlyTimes
                .FirstOrDefaultAsync(x =>
                    x.PactStaffId   == entity.PactStaffId   &&
                    x.TimeCode      == entity.TimeCode      &&
                    x.Month         == entity.Month         &&
                    x.ParentProject == entity.ParentProject);

            if (existing is null)
            {
                _dbContext.MonthlyTimes.Add(entity);
            }
            else
            {
                existing.Hours     = entity.Hours;
                existing.WorkGroup = entity.WorkGroup;
            }

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(
            string pactStaffId, string timeCode, double month, string parentProject)
        {
            var entity = await _dbContext.MonthlyTimes
                .FirstOrDefaultAsync(x =>
                    x.PactStaffId   == pactStaffId   &&
                    x.TimeCode      == timeCode      &&
                    x.Month         == month         &&
                    x.ParentProject == parentProject);

            if (entity is null) return false;

            _dbContext.MonthlyTimes.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
