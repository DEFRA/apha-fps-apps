/*
 * TRANSFORMENGINE MIGRATION — TestListVlaService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend service implementation created as a thin delegate to IFpsApiClient.FpsTestListVla
 *   - Implements ITestListVlaService; injects IFpsApiClient (aggregate API client)
 *   - Every method body is a single return await delegation — no business logic
 *   - Composite PK (ItemCode + FpsYear) forwarded exactly as received to the API client
 *   - fpsYear business context parameter forwarded on list and lookup calls
 *
 * PRESERVED:
 *   - Thin delegate pattern: no conditional logic, no data transformation, no mapping
 *   - _client field is private readonly (Sonar S2933 compliance)
 *   - All six method signatures from ITestListVlaService implemented
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register ITestListVlaService → TestListVlaService in
 *     Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs (Phase 9 scope).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service delegate for TestOrProduct VLA list management.
    /// Forwards all calls to IFpsApiClient.FpsTestListVla — contains NO business logic.
    /// </summary>
    public class TestListVlaService : ITestListVlaService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliance
        private readonly IFpsApiClient _client;

        public TestListVlaService(IFpsApiClient client)
        {
            _client = client;
        }

        // TRANSFORMENGINE: thin delegate — GET /api/v1/testlistvla?fpsYear={year}
        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear)
            => await _client.FpsTestListVla.GetAllAsync(query, fpsYear);

        // TRANSFORMENGINE: thin delegate — GET /api/v1/testlistvla/lookup?fpsYear={year}
        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear)
            => await _client.FpsTestListVla.GetAllByYearAsync(fpsYear);

        // TRANSFORMENGINE: thin delegate — GET /api/v1/testlistvla/{itemCode}/{fpsYear}
        public async Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear)
            => await _client.FpsTestListVla.GetByIdAsync(itemCode, fpsYear);

        // TRANSFORMENGINE: thin delegate — POST /api/v1/testlistvla
        public async Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto)
            => await _client.FpsTestListVla.CreateAsync(dto);

        // TRANSFORMENGINE: thin delegate — PUT /api/v1/testlistvla/{itemCode}/{fpsYear}
        public async Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto)
            => await _client.FpsTestListVla.UpdateAsync(itemCode, fpsYear, dto);

        // TRANSFORMENGINE: thin delegate — DELETE /api/v1/testlistvla/{itemCode}/{fpsYear}
        public async Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear)
            => await _client.FpsTestListVla.DeleteAsync(itemCode, fpsYear);
    }
}
