/*
 * TRANSFORMENGINE MIGRATION — IPimsProfitCentreManagerLinkApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ProfitCentreManagerLink CRUD endpoints
 *   - Mirrors backend ProfitCentreManagerLinkController routes:
 *       GET    /api/v1/profitcentremanagerlink                          — full list
 *       GET    /api/v1/profitcentremanagerlink/{profitcentre}           — scoped by profit centre
 *       GET    /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — composite natural PK get
 *       POST   /api/v1/profitcentremanagerlink                         — create link
 *       DELETE /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — delete by composite natural PK
 *   - Composite natural PK (profitcentre string + manager string) — URL-encoding handled by implementation
 *   - No PUT endpoint — link table has no mutable fields beyond composite PK
 *
 * PRESERVED:
 *   - Composite natural PK semantics (profitcentre + manager)
 *   - GetByProfitCentre scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm composite natural PK delete route with URL-encoded string segments is acceptable
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ProfitCentreManagerLinkController — composite natural PK (profitcentre + manager); URL-encoding in implementation
    public interface IPimsProfitCentreManagerLinkApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink — full list
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink/{profitcentre} — scoped by profit centre
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByProfitCentreAsync(string profitcentre);

        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — composite PK get
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetByIdAsync(string profitcentre, string manager);

        // TRANSFORMENGINE: POST /api/v1/profitcentremanagerlink
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateAsync(ProfitCentreManagerLinkDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/profitcentremanagerlink/{profitcentre}/{manager}
        Task<ApiResponseDto<bool>> DeleteAsync(string profitcentre, string manager);
    }
}
