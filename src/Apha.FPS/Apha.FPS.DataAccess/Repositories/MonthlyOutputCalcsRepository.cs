using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class MonthlyOutputCalcsRepository : BaseRepository, IMonthlyOutputCalcsRepository
    {
        private readonly FpsDbContext _dbContext;

        public MonthlyOutputCalcsRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<MonthlyOutputCalcsView>> GetByProjectAsync(
            PaginationParameters<string> query, string projectCode)
        {
            var result = await _dbContext.MonthlyOutputs
                .AsNoTracking()
                .Where(x => x.Buyer == projectCode)
                .Select(x => new MonthlyOutputCalcsView
                {
                    Buyer     = x.Buyer,
                    TestCode  = x.TestCode,
                    Month     = x.Month,
                    Volume    = x.Volume,
                    WorkGroup = x.WorkGroup,
                    FpsYear   = x.FpsYear ?? 0
                })
                .ToListAsync();

            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<(double TotalVolume, double TotalCost)> GetTotalActualByProjectAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return (0, 0);

            var totalVolume = await _dbContext.MonthlyOutputs
                .AsNoTracking()
                .Where(x => x.Buyer == projectCode)
                .SumAsync(x => x.Volume ?? 0);

            return (totalVolume, 0);
        }

        public async Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup)
        {
            var entity = await _dbContext.MonthlyOutputs
                .FirstOrDefaultAsync(m =>
                    m.Buyer     == buyer     &&
                    m.TestCode  == testCode  &&
                    m.Month     == month     &&
                    m.WorkGroup == workGroup);

            if (entity is null) return false;

            _dbContext.MonthlyOutputs.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
