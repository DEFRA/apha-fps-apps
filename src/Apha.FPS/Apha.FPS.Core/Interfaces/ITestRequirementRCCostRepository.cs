/*
 * TRANSFORMENGINE MIGRATION — ITestRequirementRCCostRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New repository interface created for TestRequirementRCCost (project-specific component charges) CRUD
 *   - Scoped to the project component charges tab from the fsubTestequirementRCPrice subform use case
 *   - Async-only signatures; no synchronous overloads per Core layer rules
 *   - GetByTestCodeAsync returns all buyer/profit-centre charges for a test+year (project charges tab list)
 *   - GetByKeyAsync returns single record by composite PK (testCode, buyer, profitCentre, fpsYear)
 *   - ExistsAsync supports AnyAsync-style pre-check before insert
 *   - CRUD operations match planned routes in transform-plan.md:
 *     GET /api/v1/testrequirementrccost/{testCode}/{fpsYear},
 *     POST /api/v1/testrequirementrccost,
 *     PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear},
 *     DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) — Core layer only
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FK validation (TestCode + Buyer + FpsYear in fps.tlkptestreqmt,
 *     TestCode + ProfitCentre + FpsYear in fps.tbltestrccost) must be enforced in the service layer.
 */

using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for project-specific component charges (TestRequirementRCCost) CRUD.
    /// Scoped to the fsubTestequirementRCPrice project component charges tab use case.
    /// Composite PK on fps.tbltestrequirementrccost: TestCode + Buyer + ProfitCentre + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestRequirementRCCostRepository
    {
        // TRANSFORMENGINE: List all project charges for a test+year — GET /api/v1/testrequirementrccost/{testCode}/{fpsYear}
        Task<IEnumerable<TestRequirementRCCost>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: Single record fetch by composite PK for edit/delete operations
        Task<TestRequirementRCCost?> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: AnyAsync-style pre-insert existence check — avoids composite PK violation
        Task<bool> ExistsAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testrequirementrccost — create new project charge entry
        Task<TestRequirementRCCost> AddAsync(TestRequirementRCCost testRequirementRCCost);

        // TRANSFORMENGINE: PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — update entry
        Task<TestRequirementRCCost> UpdateAsync(TestRequirementRCCost testRequirementRCCost);

        // TRANSFORMENGINE: DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — delete entry
        Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
