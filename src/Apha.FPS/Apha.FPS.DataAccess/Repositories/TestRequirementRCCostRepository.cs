using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestRequirementRCCostRepository : BaseRepository, ITestRequirementRCCostRepository
    {
        private readonly FpsDbContext _dbContext;

        public TestRequirementRCCostRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<TestRequirementRCCost>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            return await _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode && e.FpsYear == fpsYear)
                .OrderBy(e => e.Buyer)
                .ThenBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        public async Task<TestRequirementRCCost?> GetByKeyAsync(
            string testCode, string buyer, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.Buyer == buyer
                                       && e.ProfitCentre == profitCentre
                                       && e.FpsYear == fpsYear);
        }

        public async Task<bool> ExistsAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRequirementRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.Buyer == buyer
                            && e.ProfitCentre == profitCentre
                            && e.FpsYear == fpsYear);
        }

        public async Task<TestRequirementRCCost> AddAsync(TestRequirementRCCost testRequirementRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRequirementRCCosts.Add(testRequirementRCCost);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return testRequirementRCCost;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TestRequirementRCCost> UpdateAsync(TestRequirementRCCost testRequirementRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.TestRequirementRCCosts
                        .FirstOrDefaultAsync(e => e.TestCode == testRequirementRCCost.TestCode
                                               && e.Buyer == testRequirementRCCost.Buyer
                                               && e.ProfitCentre == testRequirementRCCost.ProfitCentre
                                               && e.FpsYear == testRequirementRCCost.FpsYear);

                    if (existing == null)
                        throw new KeyNotFoundException(
                            $"TestRequirementRCCost not found: TestCode='{testRequirementRCCost.TestCode}', " +
                            $"Buyer='{testRequirementRCCost.Buyer}', " +
                            $"ProfitCentre='{testRequirementRCCost.ProfitCentre}', " +
                            $"FpsYear={testRequirementRCCost.FpsYear}");

                    existing.Price = testRequirementRCCost.Price;

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

        public async Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            var entity = await _dbContext.TestRequirementRCCosts
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.Buyer == buyer
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
                    _dbContext.TestRequirementRCCosts.Remove(entity);
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
