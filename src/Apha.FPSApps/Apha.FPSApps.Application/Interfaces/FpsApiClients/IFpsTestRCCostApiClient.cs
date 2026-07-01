/*
 * TRANSFORMENGINE MIGRATION — IFpsTestRCCostApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend API client interface created for TestRCCost (fsubTestRCPrice subform)
 *   - Method signatures match backend TestRCCostController endpoints exactly
 *   - Composite PK (TestCode + ProfitCentre + FpsYear) reflected in GetByKeyAsync, UpdateAsync, DeleteAsync
 *   - testCode + fpsYear are required business context (from parent TestListVla row selection)
 *   - No paged list endpoint — backend returns a flat list for a given testCode+fpsYear
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - Backend route semantics: GET(by-testCode+year list), GET(by full PK), POST, PUT, DELETE
 *   - Composite PK parameter ordering: testCode, profitCentre, fpsYear (matches route template)
 *   - Subform resource family kept separate from main TestListVla CRUD resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm ApiDtoMapper includes TestRCCostRes → TestRCCostDto and
 *     TestRCCostDto → TestRCCostReq mappings in FpsApiDtoMapper before wiring the HTTP client.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for component charges per profit centre (TestRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testrccost
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// </summary>
    public interface IFpsTestRCCostApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/testrccost/{testCode}/{fpsYear} — list all charges for a test+year; testCode+fpsYear from parent row
        Task<ApiResponseDto<List<TestRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: GET /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — single record fetch by full composite PK
        Task<ApiResponseDto<TestRCCostDto>> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testrccost — create new component charge row
        Task<ApiResponseDto<TestRCCostDto>> CreateAsync(TestRCCostDto dto);

        // TRANSFORMENGINE: PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — update component charge by composite PK
        Task<ApiResponseDto<TestRCCostDto>> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — delete component charge by composite PK
        Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
