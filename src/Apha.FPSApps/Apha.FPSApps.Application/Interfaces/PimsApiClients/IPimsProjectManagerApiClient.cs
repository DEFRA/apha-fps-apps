/*
 * TRANSFORMENGINE MIGRATION — IPimsProjectManagerApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ProjectManager CRUD endpoints
 *   - Mirrors backend ProjectManagerController routes:
 *       GET    /api/v1/projectmanager                        — full list
 *       GET    /api/v1/projectmanager/{projectmanager}       — natural varchar PK get
 *       POST   /api/v1/projectmanager                        — create
 *       PUT    /api/v1/projectmanager/{projectmanager}       — update
 *       DELETE /api/v1/projectmanager/{projectmanager}       — delete
 *   - Natural varchar string PK (projectmanager name) — URL-encoding handled by HTTP client implementation
 *
 * PRESERVED:
 *   - Natural string PK semantics (projectmanager name as identifier)
 *   - All CRUD semantics matching ProjectManagerController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm rename scenario handling (delete+create vs update-in-place) at implementation layer
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ProjectManagerController — natural varchar PK (projectmanager); URL-encoding applied in implementation
    public interface IPimsProjectManagerApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/projectmanager — full list
        Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/projectmanager/{projectmanager}
        Task<ApiResponseDto<ProjectManagerDto>> GetByIdAsync(string projectmanager);

        // TRANSFORMENGINE: POST /api/v1/projectmanager
        Task<ApiResponseDto<ProjectManagerDto>> CreateAsync(ProjectManagerDto dto);

        // TRANSFORMENGINE: PUT /api/v1/projectmanager/{projectmanager} — route PK is authoritative
        Task<ApiResponseDto<ProjectManagerDto>> UpdateAsync(string projectmanager, ProjectManagerDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/projectmanager/{projectmanager}
        Task<ApiResponseDto<bool>> DeleteAsync(string projectmanager);
    }
}
