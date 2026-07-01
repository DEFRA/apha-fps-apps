/*
 * TRANSFORMENGINE MIGRATION — IFpsTestRequirementRCCostApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New frontend API client interface created for TestRequirementRCCost (fsubTestequirementRCPrice subform)
 *   - Method signatures match backend TestRequirementRCCostController endpoints exactly
 *   - Composite PK (TestCode + Buyer + ProfitCentre + FpsYear) reflected in GetByKeyAsync, UpdateAsync, DeleteAsync
 *   - testCode + fpsYear are required business context (from parent TestListVla row selection)
 *   - buyer is required business context (from test requirement tab row selection)
 *   - profitCentre is required business context (from RC cost subform row selection)
 *   - No paged list endpoint — backend returns a flat list for a given testCode+fpsYear
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - Backend route semantics: GET(by-testCode+year list), GET(by full 4-part PK), POST, PUT, DELETE
 *   - Composite PK parameter ordering: testCode, buyer, profitCentre, fpsYear (matches route template)
 *   - Subform resource family kept separate from TestListVla CRUD resource and from TestRCCost resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm ApiDtoMapper includes TestRequirementRCCostRes → TestRequirementRCCostDto
 *     and TestRequirementRCCostDto → TestRequirementRCCostReq mappings in FpsApiDtoMapper.
 *   - TRANSFORMENGINE TODO: buyer FK validation (fps.tlkptestreqmt) and profitCentre FK validation
 *     (fps.tbltestrccost) are service-layer responsibilities — not enforced at this interface layer.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for project-specific component charges (TestRequirementRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testrequirementrccost
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// buyer is from the test requirement tab row; profitCentre is from the RC cost subform row.
    /// </summary>
    public interface IFpsTestRequirementRCCostApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/testrequirementrccost/{testCode}/{fpsYear} — list all project charges for a test+year; testCode+fpsYear from parent row
        Task<ApiResponseDto<List<TestRequirementRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear);

        // TRANSFORMENGINE: GET /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — single record fetch by full 4-part composite PK
        Task<ApiResponseDto<TestRequirementRCCostDto>> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        // TRANSFORMENGINE: POST /api/v1/testrequirementrccost — create new project component charge row
        Task<ApiResponseDto<TestRequirementRCCostDto>> CreateAsync(TestRequirementRCCostDto dto);

        // TRANSFORMENGINE: PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — update project charge by composite PK
        Task<ApiResponseDto<TestRequirementRCCostDto>> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — delete project charge by composite PK
        Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
