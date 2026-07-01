/*
 * TRANSFORMENGINE MIGRATION — ITestRequirementRCCostService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New service interface created for TestRequirementRCCost (project-specific component charges) CRUD operations
 *   - Surfaces four operations aligned with planned API routes:
 *     GetByTestCodeAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - GetByKeyAsync added for single-record edit/delete confirmation use case
 *   - Async-only signatures; all methods return Task<T>
 *   - Uses TestRequirementRCCostDto as the internal transfer type
 *
 * PRESERVED:
 *   - Route-aligned method signatures:
 *     GET /api/v1/testrequirementrccost/{testCode}/{fpsYear},
 *     POST /api/v1/testrequirementrccost,
 *     PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear},
 *     DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Service implementation must validate that TestCode+Buyer+FpsYear
 *     exists in fps.tlkptestreqmt before insert/update.
 *   - TRANSFORMENGINE TODO: Service implementation must validate that TestCode+ProfitCentre+FpsYear
 *     exists in fps.tbltestrccost before insert/update.
 */

using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for project-specific component charges (TestRequirementRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrequirementrccost: TestCode + Buyer + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRequirementRCCostService
    {
        // TRANSFORMENGINE: List all project charges for a test+year — GET /api/v1/testrequirementrccost/{testCode}/{fpsYear}
        Task<IEnumerable<TestRequirementRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: Single record fetch by composite PK for edit/delete confirmation
        Task<TestRequirementRCCostDto?> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: Create new project charge entry — POST /api/v1/testrequirementrccost
        Task<TestRequirementRCCostDto> CreateAsync(TestRequirementRCCostDto dto);

        // TRANSFORMENGINE: Update entry — PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}
        Task<TestRequirementRCCostDto> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto);

        // TRANSFORMENGINE: Delete entry — DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}
        Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
