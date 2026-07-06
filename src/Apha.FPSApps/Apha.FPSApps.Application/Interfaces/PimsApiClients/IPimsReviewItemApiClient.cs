/*
 * TRANSFORMENGINE MIGRATION — IPimsReviewItemApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ReviewItem CRUD endpoints
 *   - Mirrors backend ReviewItemController routes:
 *       GET    /api/v1/reviewitem                — full list
 *       GET    /api/v1/reviewitem/{itemid}       — get by integer PK
 *       POST   /api/v1/reviewitem                — create
 *       PUT    /api/v1/reviewitem/{itemid}       — update; route PK is authoritative
 *       DELETE /api/v1/reviewitem/{itemid}       — delete
 *   - Integer PK (itemid) — matches backend controller route constraint {itemid:int}
 *   - Other Tab lookup CRUD from frmMaintainance
 *
 * PRESERVED:
 *   - All CRUD semantics matching ReviewItemController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer PK generation strategy — verify DB identity/sequence vs application-assigned
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReviewItemController — integer PK (itemid); full CRUD; Other Tab lookup table
    public interface IPimsReviewItemApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/reviewitem — full list
        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/reviewitem/{itemid:int}
        Task<ApiResponseDto<ReviewItemDto>> GetByIdAsync(int itemid);

        // TRANSFORMENGINE: POST /api/v1/reviewitem
        Task<ApiResponseDto<ReviewItemDto>> CreateAsync(ReviewItemDto dto);

        // TRANSFORMENGINE: PUT /api/v1/reviewitem/{itemid:int} — route PK is authoritative
        Task<ApiResponseDto<ReviewItemDto>> UpdateAsync(int itemid, ReviewItemDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/reviewitem/{itemid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int itemid);
    }
}
