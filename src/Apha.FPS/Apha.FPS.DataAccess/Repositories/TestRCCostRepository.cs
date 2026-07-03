using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestRCCostRepository : BaseRepository, ITestRCCostRepository
    {
        private readonly FpsDbContext _dbContext;

        public TestRCCostRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode && e.FpsYear == fpsYear)
                .OrderBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        public async Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.ProfitCentre == profitCentre
                                       && e.FpsYear == fpsYear);
        }

        public async Task<bool> ExistsAsync(string testCode, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.ProfitCentre == profitCentre
                            && e.FpsYear == fpsYear);
        }

        public async Task<TestRCCost> AddAsync(TestRCCost testRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRCCosts.Add(testRCCost);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return testRCCost;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TestRCCost> UpdateAsync(TestRCCost testRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.TestRCCosts
                        .FirstOrDefaultAsync(e => e.TestCode == testRCCost.TestCode
                                               && e.ProfitCentre == testRCCost.ProfitCentre
                                               && e.FpsYear == testRCCost.FpsYear);

                    if (existing == null)
                        throw new KeyNotFoundException(
                            $"TestRCCost not found: TestCode='{testRCCost.TestCode}', " +
                            $"ProfitCentre='{testRCCost.ProfitCentre}', FpsYear={testRCCost.FpsYear}");

                    existing.Price = testRCCost.Price;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existing;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear)
        {
            var entity = await _dbContext.TestRCCosts
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.ProfitCentre == profitCentre
                                       && e.FpsYear == fpsYear);

            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRCCosts.Remove(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
