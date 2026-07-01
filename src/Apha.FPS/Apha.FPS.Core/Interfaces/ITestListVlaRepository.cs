/*
 * TRANSFORMENGINE MIGRATION — ITestListVlaRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New repository interface created for TestOrProduct VLA list operations
 *   - Scoped to VLA (Value-Added Laboratory) use case from frmTestList / fsubTest_MainList
 *   - Async-only signatures; no synchronous overloads per Core layer rules
 *   - GetPagedAsync supports filtered/paged list for the main grid (testcode, fpsyear lookup)
 *   - GetByKeyAsync returns a single TestOrProduct by composite PK (itemCode + fpsYear)
 *   - ExistsAsync supports AnyAsync-style pre-check before insert (avoids duplicate PK violation)
 *   - CRUD operations (Add, Update, Delete) match the routes planned in transform-plan.md:
 *     POST /api/v1/testlistvla, PUT /api/v1/testlistvla/{itemCode}/{fpsYear},
 *     DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) — Core layer only
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm filter/search fields for GetPagedAsync with
 *     the Application service layer (Phase 3) — string filter is a prefix search placeholder.
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for TestOrProduct VLA list operations.
    /// Scoped to the frmTestList / fsubTest_MainList VLA use case.
    /// Composite PK on fps.testorproduct: ItemCode + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestListVlaRepository
    {
        // TRANSFORMENGINE: Paged list for main grid — GET /api/v1/testlistvla/{fpsYear}
        //   string filter used as search prefix across itemcode / itemdescription
        Task<PagedData<TestOrProduct>> GetPagedAsync(PaginationParameters<string> query, int fpsYear);

        // TRANSFORMENGINE: All items for a given year (unpaged) — used by lookup / select lists
        Task<IEnumerable<TestOrProduct>> GetAllByYearAsync(int fpsYear);

        // TRANSFORMENGINE: Single record fetch by composite PK for edit/view operations
        Task<TestOrProduct?> GetByKeyAsync(string itemCode, int fpsYear);

        // TRANSFORMENGINE: AnyAsync-style pre-insert existence check — avoids PK violation
        Task<bool> ExistsAsync(string itemCode, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testlistvla — create new TestOrProduct VLA entry
        Task<TestOrProduct> AddAsync(TestOrProduct testOrProduct);

        // TRANSFORMENGINE: PUT /api/v1/testlistvla/{itemCode}/{fpsYear} — update existing entry
        Task<TestOrProduct> UpdateAsync(TestOrProduct testOrProduct);

        // TRANSFORMENGINE: DELETE /api/v1/testlistvla/{itemCode}/{fpsYear} — delete entry
        Task<bool> DeleteAsync(string itemCode, int fpsYear);
    }
}
