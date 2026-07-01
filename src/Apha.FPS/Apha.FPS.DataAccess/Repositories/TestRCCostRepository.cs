/*
 * TRANSFORMENGINE MIGRATION — TestRCCostRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New LINQ-first repository implementing ITestRCCostRepository for TestRCCost CRUD
 *   - Scoped to the component charges tab from fsubTestRCPrice (fps.tbltestrccost)
 *   - GetByTestCodeAsync: AsNoTracking list for all profit-centre charges for a given test+year
 *   - GetByKeyAsync: single record by composite PK (testCode, profitCentre, fpsYear)
 *   - ExistsAsync: AnyAsync composite PK guard before insert
 *   - AddAsync / UpdateAsync / DeleteAsync: execution-strategy + transaction per project pattern
 *   - fpsYear included explicitly in all WHERE predicates for PostgreSQL partition pruning
 *
 * PRESERVED:
 *   - All ITestRCCostRepository method signatures from Phase 2
 *   - BaseRepository base class for potential paging helpers
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode + FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) must be enforced in the service layer.
 */

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

        // TRANSFORMENGINE: List all profit-centre charges for a test+year — GET /api/v1/testrccost/{testCode}/{fpsYear}
        public async Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode && e.FpsYear == fpsYear)
                .OrderBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        // TRANSFORMENGINE: Single record by composite PK (testCode, profitCentre, fpsYear)
        public async Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.ProfitCentre == profitCentre
                                       && e.FpsYear == fpsYear);
        }

        // TRANSFORMENGINE: AnyAsync pre-insert guard — avoids duplicate composite PK violation
        public async Task<bool> ExistsAsync(string testCode, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.ProfitCentre == profitCentre
                            && e.FpsYear == fpsYear);
        }

        // TRANSFORMENGINE: POST /api/v1/testrccost — create new component charge entry
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

        // TRANSFORMENGINE: PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — update price
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

                    // TRANSFORMENGINE: Only mutable field is Price
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

        // TRANSFORMENGINE: DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — delete entry
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
