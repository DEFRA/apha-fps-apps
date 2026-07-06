/*
 * TRANSFORMENGINE MIGRATION — IPimsReportApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for Report CRUD endpoints
 *   - Mirrors backend ReportController routes: GET/POST /api/v1/report, GET/PUT/DELETE /api/v1/report/{id}
 *   - Integer PK (id) — matches backend controller route constraint {id:int}
 *   - No pagination required — Reports Tab grid loads full list
 *
 * PRESERVED:
 *   - All CRUD semantics matching ReportController actions (GetAll, GetById, Create, Update, Delete)
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm role requirements match environment-specific access policy for report management
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReportController — GET/POST /api/v1/report, GET/PUT/DELETE /api/v1/report/{id:int}
    public interface IPimsReportApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/report — no required params; full list for Reports Tab grid
        Task<ApiResponseDto<List<ReportDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/report/{id:int}
        Task<ApiResponseDto<ReportDto>> GetByIdAsync(int id);

        // TRANSFORMENGINE: POST /api/v1/report
        Task<ApiResponseDto<ReportDto>> CreateAsync(ReportDto dto);

        // TRANSFORMENGINE: PUT /api/v1/report/{id:int}
        Task<ApiResponseDto<ReportDto>> UpdateAsync(int id, ReportDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/report/{id:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
