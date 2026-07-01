/*
 * TRANSFORMENGINE MIGRATION — ITestRCCostService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New service interface created for TestRCCost (component charges per profit centre) CRUD operations
 *   - Surfaces four operations aligned with planned API routes:
 *     GetByTestCodeAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - GetByKeyAsync added for single-record edit/delete confirmation use case
 *   - Async-only signatures; all methods return Task<T>
 *   - Uses TestRCCostDto as the internal transfer type
 *
 * PRESERVED:
 *   - Route-aligned method signatures:
 *     GET /api/v1/testrccost/{testCode}/{fpsYear},
 *     POST /api/v1/testrccost,
 *     PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear},
 *     DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Service implementation must validate that TestCode + FpsYear
 *     exists in fps.testorproduct before insert/update.
 *   - TRANSFORMENGINE TODO: Service implementation must validate ProfitCentre FK to
 *     fps.tblkpprofitcentre before insert/update.
 */

using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for component charges per profit centre (TestRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRCCostService
    {
        // TRANSFORMENGINE: List all profit-centre charges for a test+year — GET /api/v1/testrccost/{testCode}/{fpsYear}
        Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: Single record fetch by composite PK for edit/delete confirmation
        Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: Create new component charge entry — POST /api/v1/testrccost
        Task<TestRCCostDto> CreateAsync(TestRCCostDto dto);

        // TRANSFORMENGINE: Update entry — PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
        Task<TestRCCostDto> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto);

        // TRANSFORMENGINE: Delete entry — DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}
        Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
