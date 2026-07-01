/*
 * TRANSFORMENGINE MIGRATION — IFpsTestListVlaApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend API client interface created for TestListVla (frmTestList / fsubTest_MainList)
 *   - Method signatures match backend TestListVlaController endpoints exactly
 *   - Composite PK (ItemCode + FpsYear) reflected in GetByIdAsync, UpdateAsync, DeleteAsync signatures
 *   - fpsYear required business context parameter (from page year-selector) carried explicitly on all list/lookup methods
 *   - Lookup endpoint (GetAllByYearAsync) kept separate from CRUD resource family — mirrors /testlistvla/lookup route
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - Backend route semantics: GET(paged+fpsYear), GET(lookup+fpsYear), GET(by-key), POST, PUT, DELETE
 *   - Required business parameter: fpsYear (always required — satisfiable from page year-selector control)
 *   - Composite PK parameter ordering: itemCode before fpsYear (matches route template)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm ApiDtoMapper includes TestListVlaRes → TestListVlaDto and
 *     TestListVlaDto → TestListVlaReq mappings in FpsApiDtoMapper before wiring the HTTP client.
 *   - TRANSFORMENGINE TODO: Confirm PaginationRes<TestListVlaRes> → ApiResponseDto<List<TestListVlaDto>>
 *     mapping is handled in the HTTP client implementation (FpsTestListVlaApiClient).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for TestOrProduct VLA list management.
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testlistvla
    /// and lookup: GET /api/v1/testlistvla/lookup
    /// Composite PK: ItemCode + FpsYear.
    /// </summary>
    public interface IFpsTestListVlaApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/testlistvla?fpsYear={year} — paged list; fpsYear required from page year-selector
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        // TRANSFORMENGINE: GET /api/v1/testlistvla/lookup?fpsYear={year} — unpaged lookup list for select-list population
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear);

        // TRANSFORMENGINE: GET /api/v1/testlistvla/{itemCode}/{fpsYear} — single record fetch by composite PK
        Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testlistvla — create new VLA test record
        Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto);

        // TRANSFORMENGINE: PUT /api/v1/testlistvla/{itemCode}/{fpsYear} — update VLA test record by composite PK
        Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/testlistvla/{itemCode}/{fpsYear} — delete VLA test record by composite PK
        Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear);
    }
}
