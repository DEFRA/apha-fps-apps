/*
 * TRANSFORMENGINE MIGRATION — IPimsReportGroupLinkApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ReportGroupLink CRUD endpoints
 *   - Mirrors backend ReportGroupLinkController routes:
 *       GET    /api/v1/reportgrouplink                     — full list
 *       GET    /api/v1/reportgrouplink/{reportid}          — scoped by report
 *       GET    /api/v1/reportgrouplink/{reportid}/{groupid}— composite PK lookup
 *       POST   /api/v1/reportgrouplink                     — create link
 *       DELETE /api/v1/reportgrouplink/{reportid}/{groupid}— delete by composite PK
 *   - Composite PK (reportid int + groupid int) — no PUT endpoint (link rows have no updatable fields)
 *
 * PRESERVED:
 *   - Composite PK semantics (reportid + groupid)
 *   - GetByReportId scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm composite delete route strategy is acceptable (DELETE /reportgrouplink/{reportid}/{groupid})
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReportGroupLinkController — composite PK (reportid + groupid); no PUT (link has no mutable fields)
    public interface IPimsReportGroupLinkApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/reportgrouplink — full list
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/reportgrouplink/{reportid:int} — scoped by report
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetByReportIdAsync(int reportid);

        // TRANSFORMENGINE: GET /api/v1/reportgrouplink/{reportid:int}/{groupid:int} — composite PK get
        Task<ApiResponseDto<ReportGroupLinkDto>> GetByIdAsync(int reportid, int groupid);

        // TRANSFORMENGINE: POST /api/v1/reportgrouplink
        Task<ApiResponseDto<ReportGroupLinkDto>> CreateAsync(ReportGroupLinkDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/reportgrouplink/{reportid:int}/{groupid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int reportid, int groupid);
    }
}
