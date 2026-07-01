/*
 * TRANSFORMENGINE MIGRATION — ITestListVlaService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New service interface created for TestOrProduct VLA list business operations
 *   - Surfaces the five operations planned in transform-plan.md:
 *     GetAllAsync (paged list by year), GetByKeyAsync (single item), CreateAsync, UpdateAsync, DeleteAsync
 *   - GetAllByYearAsync added for unpaged lookup/select-list use case
 *   - Async-only signatures; all methods return Task<T>
 *   - Uses TestListVlaDto as the internal transfer type; controllers map to/from Res/Req contracts
 *
 * PRESERVED:
 *   - Route-aligned method signatures matching planned API surface:
 *     GET /api/v1/testlistvla, GET /api/v1/testlistvla/{itemCode}/{fpsYear},
 *     POST /api/v1/testlistvla, PUT /api/v1/testlistvla/{itemCode}/{fpsYear},
 *     DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: owner field validation (PT/PA/SD/LT) is expected in the service
 *     implementation — ensure service throws ArgumentException for invalid values before persisting.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for TestOrProduct VLA list business operations.
    /// Orchestrates repository calls and enforces business rules extracted from VBA/SP guards.
    /// Composite PK on fps.testorproduct: ItemCode + FpsYear.
    /// </summary>
    public interface ITestListVlaService
    {
        // TRANSFORMENGINE: Paged list — GET /api/v1/testlistvla?fpsYear={year}
        Task<PaginatedResult<TestListVlaDto>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        // TRANSFORMENGINE: Unpaged list — lookup/select-list use case by year
        Task<IEnumerable<TestListVlaDto>> GetAllByYearAsync(int fpsYear);

        // TRANSFORMENGINE: Single record fetch — GET /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<TestListVlaDto?> GetByKeyAsync(string itemCode, int fpsYear);

        // TRANSFORMENGINE: Create — POST /api/v1/testlistvla
        Task<TestListVlaDto> CreateAsync(TestListVlaDto dto);

        // TRANSFORMENGINE: Update — PUT /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<TestListVlaDto> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto);

        // TRANSFORMENGINE: Delete — DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<bool> DeleteAsync(string itemCode, int fpsYear);
    }
}
