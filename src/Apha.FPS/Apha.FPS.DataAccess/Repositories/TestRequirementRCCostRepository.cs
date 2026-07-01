/*
 * TRANSFORMENGINE MIGRATION — TestRequirementRCCostRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New LINQ-first repository implementing ITestRequirementRCCostRepository for
 *     TestRequirementRCCost CRUD
 *   - Scoped to the project component charges tab from fsubTestequirementRCPrice
 *     (fps.tbltestrequirementrccost)
 *   - GetByTestCodeAsync: AsNoTracking list for all buyer/profit-centre charges for a given test+year
 *   - GetByKeyAsync: single record by composite PK (testCode, buyer, profitCentre, fpsYear)
 *   - ExistsAsync: AnyAsync composite PK guard before insert
 *   - AddAsync / UpdateAsync / DeleteAsync: execution-strategy + transaction per project pattern
 *   - fpsYear included explicitly in all WHERE predicates for PostgreSQL partition pruning
 *
 * PRESERVED:
 *   - All ITestRequirementRCCostRepository method signatures from Phase 2
 *   - BaseRepository base class for potential paging helpers
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode + Buyer + FpsYear in fps.tlkptestreqmt,
 *     TestCode + ProfitCentre + FpsYear in fps.tbltestrccost) must be enforced in service layer.
 */

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

        // TRANSFORMENGINE: List all project charges for a test+year — GET /api/v1/testrequirementrccost/{testCode}/{fpsYear}
        public async Task<IEnumerable<TestRequirementRCCost>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            return await _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode && e.FpsYear == fpsYear)
                .OrderBy(e => e.Buyer)
                .ThenBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        // TRANSFORMENGINE: Single record by composite PK (testCode, buyer, profitCentre, fpsYear)
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

        // TRANSFORMENGINE: AnyAsync pre-insert guard — avoids duplicate composite PK violation
        public async Task<bool> ExistsAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            return await _dbContext.TestRequirementRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.Buyer == buyer
                            && e.ProfitCentre == profitCentre
                            && e.FpsYear == fpsYear);
        }

        // TRANSFORMENGINE: POST /api/v1/testrequirementrccost — create new project charge entry
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

        // TRANSFORMENGINE: PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — update price
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

                    // TRANSFORMENGINE: Only mutable field is Price
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

        // TRANSFORMENGINE: DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — delete entry
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
