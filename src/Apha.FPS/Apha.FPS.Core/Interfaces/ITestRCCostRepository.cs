/*
 * TRANSFORMENGINE MIGRATION — ITestRCCostRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New repository interface created for TestRCCost (component charges per profit centre) CRUD
 *   - Scoped to the component charges tab from the fsubTestRCPrice subform use case
 *   - Async-only signatures; no synchronous overloads per Core layer rules
 *   - GetByTestCodeAsync returns all profit-centre charges for a test/year (component tab list)
 *   - GetByKeyAsync returns single record by composite PK (testCode, profitCentre, fpsYear)
 *   - ExistsAsync supports AnyAsync-style pre-check before insert
 *   - CRUD operations match planned routes in transform-plan.md:
 *     GET /api/v1/testrccost/{testCode}/{fpsYear},
 *     POST /api/v1/testrccost,
 *     PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear},
 *     DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) — Core layer only
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode + FpsYear in fps.testorproduct,
 *     ProfitCentre in fps.tblkpprofitcentre) must be enforced in the service layer.
 */

using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for component charges per profit centre (TestRCCost) CRUD.
    /// Scoped to the fsubTestRCPrice component charges tab use case.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestRCCostRepository
    {
        // TRANSFORMENGINE: List all profit-centre charges for a test+year — GET /api/v1/testrccost/{testCode}/{fpsYear}
        Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: Single record fetch by composite PK for edit/delete operations
        Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: AnyAsync-style pre-insert existence check — avoids composite PK violation
        Task<bool> ExistsAsync(string testCode, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testrccost — create new component charge entry
        Task<TestRCCost> AddAsync(TestRCCost testRCCost);

        // TRANSFORMENGINE: PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — update entry
        Task<TestRCCost> UpdateAsync(TestRCCost testRCCost);

        // TRANSFORMENGINE: DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — delete entry
        Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
