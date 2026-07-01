/*
 * TRANSFORMENGINE MIGRATION — ITestListVlaService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend service interface created for TestOrProduct VLA list management
 *   - Method signatures mirror IFpsTestListVlaApiClient exactly
 *   - Composite PK (ItemCode + FpsYear) reflected in GetByIdAsync, UpdateAsync, DeleteAsync
 *   - fpsYear required business context parameter carried explicitly on GetAllAsync and GetAllByYearAsync
 *   - Lookup method (GetAllByYearAsync) kept separate from CRUD — corresponds to /testlistvla/lookup route
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - Backend route semantics: GET(paged+fpsYear), GET(lookup+fpsYear), GET(by-key), POST, PUT, DELETE
 *   - Required business parameter: fpsYear (always required — satisfiable from page year-selector control)
 *   - Composite PK parameter ordering: itemCode before fpsYear
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that MVC controller injects ITestListVlaService (not IFpsTestListVlaApiClient directly).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for TestOrProduct VLA list management.
    /// Thin delegate surface — all methods forward to IFpsApiClient.FpsTestListVla.
    /// Backend routes: GET/POST/PUT/DELETE /api/v1/testlistvla and GET /api/v1/testlistvla/lookup.
    /// Composite PK: ItemCode + FpsYear.
    /// </summary>
    public interface ITestListVlaService
    {
        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.GetAllAsync — GET /api/v1/testlistvla?fpsYear={year}
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.GetAllByYearAsync — GET /api/v1/testlistvla/lookup?fpsYear={year}
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear);

        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.GetByIdAsync — GET /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear);

        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.CreateAsync — POST /api/v1/testlistvla
        Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto);

        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.UpdateAsync — PUT /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto);

        // TRANSFORMENGINE: mirrors IFpsTestListVlaApiClient.DeleteAsync — DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
        Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear);
    }
}
